// =============================================================
// modules/storage.bicep
// Azure Storage Account + 静的サイトホスティング設定
// =============================================================

// ----- パラメータ -----

@description('デプロイ環境')
param environment string

@description('プロジェクト識別子')
param projectName string

@description('Azureリージョン')
param location string

@description('インデックスドキュメント名')
param indexDocument string

@description('エラードキュメント名')
param errorDocument string

@description('許可するIPアドレス範囲')
param allowedIpRanges array

// ----- 変数 -----

// ストレージアカウント名: 小文字英数字のみ・最大24文字
// 例: kintaidevstor, kintaistgstor, kintaiprd<uniqueSuffix>
var storageAccountName = take(
  toLower('${projectName}${environment}${uniqueString(resourceGroup().id)}'),
  24
)

var networkAclsDefaultAction = empty(allowedIpRanges) ? 'Allow' : 'Deny'

var ipRules = [for ip in allowedIpRanges: {
  value: ip
  action: 'Allow'
}]

// ----- リソース -----

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: true   // 静的サイト公開に必要
    allowSharedKeyAccess: true
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: networkAclsDefaultAction
      bypass: 'AzureServices'
      ipRules: ipRules
      virtualNetworkRules: []
    }
  }
  tags: {
    environment: environment
    project:     projectName
    managedBy:   'bicep'
  }
}

// 静的サイトホスティングの有効化
// NOTE: Bicep では staticWebsite は blobServices 経由で設定できないため
//       deploymentScript または Azure CLI を使うか、
//       storageAccounts/blobServices の properties で設定する。
//       2023-05-01 API では staticWebsite は storageAccounts の
//       プロパティとして直接設定不可のため、blobServices リソースで対応。
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
    deleteRetentionPolicy: {
      enabled: false
    }
  }
}

// $web コンテナ（静的サイトホスティング用の特殊コンテナ）
resource webContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: '$web'
  properties: {
    publicAccess: 'None'  // 静的サイト経由のみアクセス可（コンテナ直接アクセス不可）
  }
}

// ----- 出力 -----
// NOTE: 静的サイトホスティングの有効化は deploymentScripts では
//       テナントポリシー (キー認証禁止) により実行できないため、
//       Bicep デプロイ後に Azure CLI で別途実施する。
//       az storage blob service-properties update \
//         --account-name <name> --static-website \
//         --index-document index.html --404-document index.html \
//         --auth-mode login

@description('インデックスドキュメント名 (デプロイ後の CLI 手順用参照値)')
output indexDocument string = indexDocument

@description('エラードキュメント名 (デプロイ後の CLI 手順用参照値)')
output errorDocument string = errorDocument

@description('ストレージアカウント名')
output storageAccountName string = storageAccount.name

@description('静的サイトのプライマリエンドポイント')
output staticWebsiteUrl string = storageAccount.properties.primaryEndpoints.web

@description('Blob サービスエンドポイント')
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob

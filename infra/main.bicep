// =============================================================
// main.bicep
// 静的サイトホスティング用 Azure Blob Storage のデプロイ
// =============================================================

targetScope = 'resourceGroup'

// ----- パラメータ -----

@description('デプロイ環境 (dev / stg / prd)')
@allowed(['dev', 'stg', 'prd'])
param environment string = 'dev'

@description('プロジェクト識別子 (リソース名のプレフィックスに使用)')
@minLength(2)
@maxLength(8)
param projectName string = 'kintai'

@description('Azureリージョン')
param location string = resourceGroup().location

@description('静的サイトのインデックスドキュメント名')
param indexDocument string = 'index.html'

@description('静的サイトの404エラードキュメント名')
param errorDocument string = 'index.html'

@description('許可するIPアドレス範囲 (空の場合はパブリックアクセス許可)')
param allowedIpRanges array = []

// ----- モジュール呼び出し -----

module storage 'modules/storage.bicep' = {
  name: 'storage-deploy'
  params: {
    environment:     environment
    projectName:     projectName
    location:        location
    indexDocument:   indexDocument
    errorDocument:   errorDocument
    allowedIpRanges: allowedIpRanges
  }
}

// ----- 出力 -----

@description('ストレージアカウント名')
output storageAccountName string = storage.outputs.storageAccountName

@description('静的サイトのプライマリエンドポイント URL')
output staticWebsiteUrl string = storage.outputs.staticWebsiteUrl

@description('$web コンテナへのアップロード用 Blob サービスエンドポイント')
output blobEndpoint string = storage.outputs.blobEndpoint

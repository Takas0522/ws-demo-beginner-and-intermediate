// =============================================================
// main.bicepparam
// 環境ごとのパラメータ値 (dev 向けサンプル)
// =============================================================

using './main.bicep'

param environment  = 'dev'
param projectName  = 'kintai'
param location     = 'japaneast'
param indexDocument = 'index.html'
param errorDocument = 'index.html'
param allowedIpRanges = []   // 空 = パブリックアクセス許可

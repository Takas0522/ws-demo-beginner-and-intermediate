#!/usr/bin/env node
/**
 * collect-data.js
 * 開発状況ダッシュボード (http://localhost:3002/) から
 * チケット一覧 CSV をダウンロードして temp/collected-data.csv に保存する。
 *
 * 使い方:
 *   npm install          # 初回のみ
 *   node collect-data.js
 *
 * 環境変数（省略時はデフォルト値を使用）:
 *   APP_URL       アプリの URL          (デフォルト: http://localhost:3002)
 *   APP_USERNAME  ログインユーザー名    (デフォルト: game_dev)
 *   APP_PASSWORD  ログインパスワード    (デフォルト: password123)
 *   OUTPUT_DIR    CSV の保存先ディレクトリ (デフォルト: ../../temp)
 */

const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

const BASE_URL = process.env.APP_URL || 'http://localhost:3002';
const USERNAME = process.env.APP_USERNAME || 'game_dev';
const PASSWORD = process.env.APP_PASSWORD || 'password123';
const OUTPUT_DIR = path.resolve(
  process.env.OUTPUT_DIR || path.join(__dirname, '../../temp')
);
const OUTPUT_FILE = path.join(OUTPUT_DIR, 'collected-data.csv');

async function main() {
  // 出力ディレクトリが存在しない場合は作成
  fs.mkdirSync(OUTPUT_DIR, { recursive: true });

  console.log(`🌐 接続先: ${BASE_URL}`);
  console.log(`📁 保存先: ${OUTPUT_FILE}`);

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext();
  const page = await context.newPage();

  try {
    // ────────────────────────────────────────
    // 1. ログイン
    // ────────────────────────────────────────
    console.log('🔑 ログイン中...');
    await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });

    await page.fill('input[placeholder="username"]', USERNAME);
    await page.fill('input[type="password"]', PASSWORD);
    await page.press('input[type="password"]', 'Enter');

    // ダッシュボードのトップページ（SummaryPage）が表示されるまで待機
    await page.waitForURL(`${BASE_URL}/`, { timeout: 15000 });
    console.log('✅ ログイン完了');

    // ────────────────────────────────────────
    // 2. CSV ダウンロード
    // ────────────────────────────────────────
    console.log('📥 CSV ダウンロード中...');
    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 30000 }),
      page.click('button:has-text("チケット明細 CSV")'),
    ]);

    // ────────────────────────────────────────
    // 3. ファイル保存
    // ────────────────────────────────────────
    await download.saveAs(OUTPUT_FILE);
    const stat = fs.statSync(OUTPUT_FILE);
    console.log(`✅ 保存完了: ${OUTPUT_FILE} (${stat.size.toLocaleString()} bytes)`);

  } finally {
    await browser.close();
  }
}

main().catch(err => {
  console.error('❌ エラー:', err.message);
  process.exit(1);
});

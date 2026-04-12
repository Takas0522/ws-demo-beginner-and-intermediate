-- ============================================================
-- App1: サービスダッシュボード シードデータ
-- 複合企業が提供する多様な電子サービスのダミーデータ
-- ============================================================

-- ────────────────────────────────────────
-- 事業部 (5部門)
-- ────────────────────────────────────────
INSERT INTO business_units (id, name, description) VALUES
  ('b0000001-0000-0000-0000-000000000001', 'エンタープライズSaaS事業部',    'BtoB向けクラウドサービスを展開'),
  ('b0000002-0000-0000-0000-000000000002', 'コンシューマーゲーム事業部',    'スマートフォン・PC向けゲームを開発・運営'),
  ('b0000003-0000-0000-0000-000000000003', 'メディア・コンテンツ事業部',    '動画・音楽・電子書籍配信サービスを提供'),
  ('b0000004-0000-0000-0000-000000000004', 'フィンテック事業部',            '決済・送金・家計管理サービスを展開'),
  ('b0000005-0000-0000-0000-000000000005', 'ヘルスケア・ウェルネス事業部', '健康管理・フィットネスアプリを提供');

-- ────────────────────────────────────────
-- サービスカテゴリ
-- ────────────────────────────────────────
INSERT INTO service_categories (id, name, description) VALUES
  ('c0000001-0000-0000-0000-000000000001', 'SaaS',         'Software as a Service'),
  ('c0000002-0000-0000-0000-000000000002', 'ゲーム',       'スマートフォン・PC向けゲーム'),
  ('c0000003-0000-0000-0000-000000000003', '動画配信',     'VOD / ライブ配信'),
  ('c0000004-0000-0000-0000-000000000004', '音楽配信',     '楽曲ストリーミング'),
  ('c0000005-0000-0000-0000-000000000005', '電子書籍',     '漫画・小説・雑誌'),
  ('c0000006-0000-0000-0000-000000000006', 'フィンテック',  '決済・金融サービス'),
  ('c0000007-0000-0000-0000-000000000007', 'ヘルスケア',   '健康管理・医療IT');

-- ────────────────────────────────────────
-- サービス (5事業部 × 3〜4サービス)
-- ────────────────────────────────────────
INSERT INTO services (id, business_unit_id, category_id, name, description, launched_at, status) VALUES
  -- エンタープライズSaaS事業部
  ('e0000001-0000-0000-0000-000000000001', 'b0000001-0000-0000-0000-000000000001', 'c0000001-0000-0000-0000-000000000001', 'CloudHR Pro',        'クラウド型人事・給与管理SaaS',         '2020-04-01', 'active'),
  ('e0000002-0000-0000-0000-000000000002', 'b0000001-0000-0000-0000-000000000001', 'c0000001-0000-0000-0000-000000000001', 'SmartCRM',           '中小企業向けCRM・営業支援ツール',      '2021-07-01', 'active'),
  ('e0000003-0000-0000-0000-000000000003', 'b0000001-0000-0000-0000-000000000001', 'c0000001-0000-0000-0000-000000000001', 'DocFlow',            'クラウド文書管理・ワークフロー自動化', '2022-01-15', 'active'),
  -- コンシューマーゲーム事業部
  ('e0000004-0000-0000-0000-000000000004', 'b0000002-0000-0000-0000-000000000002', 'c0000002-0000-0000-0000-000000000002', 'FantasyQuest Mobile','本格RPGスマートフォンゲーム',          '2019-10-01', 'active'),
  ('e0000005-0000-0000-0000-000000000005', 'b0000002-0000-0000-0000-000000000002', 'c0000002-0000-0000-0000-000000000002', 'PuzzleBurst',        'カジュアルパズルゲーム',               '2021-03-01', 'active'),
  ('e0000006-0000-0000-0000-000000000006', 'b0000002-0000-0000-0000-000000000002', 'c0000002-0000-0000-0000-000000000002', 'RacingKing Online',  'PCオンラインレーシングゲーム',         '2020-06-01', 'suspended'),
  -- メディア・コンテンツ事業部
  ('e0000007-0000-0000-0000-000000000007', 'b0000003-0000-0000-0000-000000000003', 'c0000003-0000-0000-0000-000000000003', 'StreamNow',          '動画ストリーミングサービス',           '2018-09-01', 'active'),
  ('e0000008-0000-0000-0000-000000000008', 'b0000003-0000-0000-0000-000000000003', 'c0000004-0000-0000-0000-000000000004', 'MelodyBox',          '音楽ストリーミングサービス',           '2019-04-01', 'active'),
  ('e0000009-0000-0000-0000-000000000009', 'b0000003-0000-0000-0000-000000000003', 'c0000005-0000-0000-0000-000000000005', 'ComicShelf',         '電子漫画・書籍配信',                   '2020-11-01', 'active'),
  ('e000000a-0000-0000-0000-00000000000a', 'b0000003-0000-0000-0000-000000000003', 'c0000003-0000-0000-0000-000000000003', 'LiveStage',          'ライブ配信プラットフォーム',           '2022-05-01', 'active'),
  -- フィンテック事業部
  ('e000000b-0000-0000-0000-00000000000b', 'b0000004-0000-0000-0000-000000000004', 'c0000006-0000-0000-0000-000000000006', 'PayEasy',            'QRコード決済・送金サービス',           '2019-07-01', 'active'),
  ('e000000c-0000-0000-0000-00000000000c', 'b0000004-0000-0000-0000-000000000004', 'c0000006-0000-0000-0000-000000000006', 'MoneyNote',          'AI家計簿・資産管理アプリ',             '2020-02-01', 'active'),
  ('e000000d-0000-0000-0000-00000000000d', 'b0000004-0000-0000-0000-000000000004', 'c0000006-0000-0000-0000-000000000006', 'InvestGuide',        'ロボアドバイザー投資サービス',         '2021-10-01', 'active'),
  -- ヘルスケア・ウェルネス事業部
  ('e000000e-0000-0000-0000-00000000000e', 'b0000005-0000-0000-0000-000000000005', 'c0000007-0000-0000-0000-000000000007', 'FitTracker',         'フィットネス・運動記録アプリ',         '2020-01-01', 'active'),
  ('e000000f-0000-0000-0000-00000000000f', 'b0000005-0000-0000-0000-000000000005', 'c0000007-0000-0000-0000-000000000007', 'HealthDiary',        '健康日記・バイタル記録アプリ',         '2021-06-01', 'active'),
  ('e0000010-0000-0000-0000-000000000010', 'b0000005-0000-0000-0000-000000000005', 'c0000007-0000-0000-0000-000000000007', 'SleepWell',          '睡眠分析・改善サポートアプリ',         '2022-09-01', 'active');

-- ────────────────────────────────────────
-- サービスプラン
-- ────────────────────────────────────────
-- CloudHR Pro
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f0000001-0000-0000-0000-000000000001', 'e0000001-0000-0000-0000-000000000001', 'Trial',      0,       false),
  ('f0000002-0000-0000-0000-000000000002', 'e0000001-0000-0000-0000-000000000001', 'Standard', 9800,     true),
  ('f0000003-0000-0000-0000-000000000003', 'e0000001-0000-0000-0000-000000000001', 'Pro',      29800,    true),
  ('f0000004-0000-0000-0000-000000000004', 'e0000001-0000-0000-0000-000000000001', 'Enterprise',98000,   true);
-- SmartCRM
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f0000005-0000-0000-0000-000000000005', 'e0000002-0000-0000-0000-000000000002', 'Free',      0,       false),
  ('f0000006-0000-0000-0000-000000000006', 'e0000002-0000-0000-0000-000000000002', 'Basic',   3980,      true),
  ('f0000007-0000-0000-0000-000000000007', 'e0000002-0000-0000-0000-000000000002', 'Pro',    12800,      true);
-- DocFlow
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f0000008-0000-0000-0000-000000000008', 'e0000003-0000-0000-0000-000000000003', 'Free',      0,       false),
  ('f0000009-0000-0000-0000-000000000009', 'e0000003-0000-0000-0000-000000000003', 'Business', 5980,     true),
  ('f000000a-0000-0000-0000-00000000000a', 'e0000003-0000-0000-0000-000000000003', 'Enterprise',19800,   true);
-- FantasyQuest Mobile
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f000000b-0000-0000-0000-00000000000b', 'e0000004-0000-0000-0000-000000000004', '無料',       0,      false),
  ('f000000c-0000-0000-0000-00000000000c', 'e0000004-0000-0000-0000-000000000004', '月額パス', 960,      true);
-- PuzzleBurst
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f000000d-0000-0000-0000-00000000000d', 'e0000005-0000-0000-0000-000000000005', '無料',       0,      false),
  ('f000000e-0000-0000-0000-00000000000e', 'e0000005-0000-0000-0000-000000000005', '広告非表示', 360,    true);
-- StreamNow
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f000000f-0000-0000-0000-00000000000f', 'e0000007-0000-0000-0000-000000000007', '無料（広告あり）', 0, false),
  ('f0000010-0000-0000-0000-000000000010', 'e0000007-0000-0000-0000-000000000007', 'スタンダード', 680, true),
  ('f0000011-0000-0000-0000-000000000011', 'e0000007-0000-0000-0000-000000000007', 'プレミアム',   980, true);
-- MelodyBox
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f0000012-0000-0000-0000-000000000012', 'e0000008-0000-0000-0000-000000000008', '無料',       0,       false),
  ('f0000013-0000-0000-0000-000000000013', 'e0000008-0000-0000-0000-000000000008', 'プレミアム', 980,     true),
  ('f0000014-0000-0000-0000-000000000014', 'e0000008-0000-0000-0000-000000000008', 'ファミリー', 1480,    true);
-- ComicShelf
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f0000015-0000-0000-0000-000000000015', 'e0000009-0000-0000-0000-000000000009', '無料（都度課金）', 0, false),
  ('f0000016-0000-0000-0000-000000000016', 'e0000009-0000-0000-0000-000000000009', '読み放題',       780, true);
-- PayEasy
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f0000017-0000-0000-0000-000000000017', 'e000000b-0000-0000-0000-00000000000b', '個人',       0,      false),
  ('f0000018-0000-0000-0000-000000000018', 'e000000b-0000-0000-0000-00000000000b', 'ビジネス', 2980,     true);
-- MoneyNote
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f0000019-0000-0000-0000-000000000019', 'e000000c-0000-0000-0000-00000000000c', '無料',       0,      false),
  ('f000001a-0000-0000-0000-00000000001a', 'e000000c-0000-0000-0000-00000000000c', 'プレミアム', 480,    true);
-- FitTracker
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f000001b-0000-0000-0000-00000000001b', 'e000000e-0000-0000-0000-00000000000e', '無料',       0,      false),
  ('f000001c-0000-0000-0000-00000000001c', 'e000000e-0000-0000-0000-00000000000e', 'Pro',       480,     true);
-- HealthDiary
INSERT INTO service_plans (id, service_id, name, price, is_paid) VALUES
  ('f000001d-0000-0000-0000-00000000001d', 'e000000f-0000-0000-0000-00000000000f', '無料',       0,      false),
  ('f000001e-0000-0000-0000-00000000001e', 'e000000f-0000-0000-0000-00000000000f', 'Plus',      380,     true);

-- ────────────────────────────────────────
-- ユーザー指標 日次集計（過去12ヶ月: 代表サービスのみ生成）
-- ────────────────────────────────────────
-- generate_series を使って CloudHR Pro の過去12ヶ月分を生成（月次データとして毎月1日に代表値を設定）
INSERT INTO user_metric_daily (service_id, date, mau, dau, new_users, churned_users, total_subscriptions)
SELECT
    'e0000001-0000-0000-0000-000000000001',
    d::DATE,
    2500  + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 0.8)::INT,
    420   + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 0.1)::INT,
    45    + (random() * 20)::INT,
    8     + (random() * 5)::INT,
    1800  + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 0.6)::INT
FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;

-- StreamNow (sv-007)
INSERT INTO user_metric_daily (service_id, date, mau, dau, new_users, churned_users, total_subscriptions)
SELECT
    'e0000007-0000-0000-0000-000000000007',
    d::DATE,
    85000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 50)::INT,
    18000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 20)::INT,
    1200  + (random() * 300)::INT,
    350   + (random() * 100)::INT,
    62000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 30)::INT
FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;

-- PayEasy (sv-011)
INSERT INTO user_metric_daily (service_id, date, mau, dau, new_users, churned_users, total_subscriptions)
SELECT
    'e000000b-0000-0000-0000-00000000000b',
    d::DATE,
    320000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 200)::INT,
    75000  + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 50)::INT,
    8000   + (random() * 1500)::INT,
    2000   + (random() * 500)::INT,
    280000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 150)::INT
FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;

-- FitTracker (sv-014)
INSERT INTO user_metric_daily (service_id, date, mau, dau, new_users, churned_users, total_subscriptions)
SELECT
    'e000000e-0000-0000-0000-00000000000e',
    d::DATE,
    45000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 80)::INT,
    9500  + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 15)::INT,
    800   + (random() * 200)::INT,
    180   + (random() * 50)::INT,
    28000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 60)::INT
FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;

-- ────────────────────────────────────────
-- 売上 日次集計（月次代表値）
-- ────────────────────────────────────────
-- CloudHR Pro Standard
INSERT INTO revenue_daily (service_id, plan_id, date, amount, subscription_count)
SELECT
    'e0000001-0000-0000-0000-000000000001', 'f0000002-0000-0000-0000-000000000002', d::DATE,
    (800 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 0.5)::INT) * 9800,
    800 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 0.5)::INT
FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;

-- CloudHR Pro Pro
INSERT INTO revenue_daily (service_id, plan_id, date, amount, subscription_count)
SELECT
    'e0000001-0000-0000-0000-000000000001', 'f0000003-0000-0000-0000-000000000003', d::DATE,
    (320 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 0.2)::INT) * 29800,
    320 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 0.2)::INT
FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;

-- StreamNow スタンダード
INSERT INTO revenue_daily (service_id, plan_id, date, amount, subscription_count)
SELECT
    'e0000007-0000-0000-0000-000000000007', 'f0000010-0000-0000-0000-000000000010', d::DATE,
    (38000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 20)::INT) * 680,
    38000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 20)::INT
FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;

-- StreamNow プレミアム
INSERT INTO revenue_daily (service_id, plan_id, date, amount, subscription_count)
SELECT
    'e0000007-0000-0000-0000-000000000007', 'f0000011-0000-0000-0000-000000000011', d::DATE,
    (24000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 10)::INT) * 980,
    24000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 10)::INT
FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;

-- PayEasy ビジネス
INSERT INTO revenue_daily (service_id, plan_id, date, amount, subscription_count)
SELECT
    'e000000b-0000-0000-0000-00000000000b', 'f0000018-0000-0000-0000-000000000018', d::DATE,
    (12000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 8)::INT) * 2980,
    12000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 8)::INT
FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;

-- FitTracker Pro
INSERT INTO revenue_daily (service_id, plan_id, date, amount, subscription_count)
SELECT
    'e000000e-0000-0000-0000-00000000000e', 'f000001c-0000-0000-0000-00000000001c', d::DATE,
    (15000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 30)::INT) * 480,
    15000 + (EXTRACT(EPOCH FROM d - '2025-04-01') / 86400 * 30)::INT
FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;

-- ────────────────────────────────────────
-- 原価 日次集計
-- ────────────────────────────────────────
INSERT INTO cost_daily (service_id, date, cost_type, amount, description)
SELECT 'e0000001-0000-0000-0000-000000000001', d::DATE, 'infrastructure', 850000,  'AWS利用料' FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;
INSERT INTO cost_daily (service_id, date, cost_type, amount, description)
SELECT 'e0000001-0000-0000-0000-000000000001', d::DATE, 'labor',          3200000, '開発・運用人件費' FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;
INSERT INTO cost_daily (service_id, date, cost_type, amount, description)
SELECT 'e0000007-0000-0000-0000-000000000007', d::DATE, 'infrastructure', 4200000, 'CDN・配信インフラ' FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;
INSERT INTO cost_daily (service_id, date, cost_type, amount, description)
SELECT 'e0000007-0000-0000-0000-000000000007', d::DATE, 'license',        1800000, 'コンテンツライセンス料' FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;
INSERT INTO cost_daily (service_id, date, cost_type, amount, description)
SELECT 'e000000b-0000-0000-0000-00000000000b', d::DATE, 'infrastructure', 2100000, 'クラウド・セキュリティ' FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;
INSERT INTO cost_daily (service_id, date, cost_type, amount, description)
SELECT 'e000000e-0000-0000-0000-00000000000e', d::DATE, 'infrastructure',  480000, 'サーバー利用料' FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;
INSERT INTO cost_daily (service_id, date, cost_type, amount, description)
SELECT 'e000000e-0000-0000-0000-00000000000e', d::DATE, 'labor',          1200000, '開発・CS人件費' FROM generate_series('2025-04-01'::DATE, '2026-03-31'::DATE, '1 month'::INTERVAL) d;

-- ────────────────────────────────────────
-- ABテスト
-- ────────────────────────────────────────
INSERT INTO ab_tests (id, service_id, name, description, primary_metric, started_at, ended_at, status) VALUES
  ('ab001000-0000-0000-0000-000000000001', 'e0000007-0000-0000-0000-000000000007', 'トップ画面レイアウトテスト',
   '動画サムネイル配置の変更によるクリック率改善', 'click_through_rate',
   '2025-10-01', '2025-11-30', 'completed'),
  ('ab002000-0000-0000-0000-000000000002', 'e000000b-0000-0000-0000-00000000000b', 'オンボーディングフローA/Bテスト',
   '新規ユーザーの初回決済完了率を向上させる', 'first_payment_conversion',
   '2025-11-01', '2025-12-31', 'completed'),
  ('ab003000-0000-0000-0000-000000000003', 'e0000001-0000-0000-0000-000000000001', 'ダッシュボードUI刷新テスト',
   '新デザインによるユーザーエンゲージメント向上', 'daily_active_rate',
   '2026-01-15', NULL, 'running'),
  ('ab004000-0000-0000-0000-000000000004', 'e000000e-0000-0000-0000-00000000000e', 'プッシュ通知タイミング最適化',
   '運動リマインド通知の最適な時刻を検証', 'workout_completion_rate',
   '2026-02-01', NULL, 'running');

-- バリアント
INSERT INTO ab_test_variants (id, ab_test_id, name, description, traffic_allocation) VALUES
  ('a0000001-0001-0000-0000-000000000001', 'ab001000-0000-0000-0000-000000000001', 'Control',     '現行レイアウト',            50.00),
  ('a0000002-0001-0000-0000-000000000002', 'ab001000-0000-0000-0000-000000000001', 'Treatment A', 'グリッド型レイアウト',      50.00),
  ('a0000003-0001-0000-0000-000000000003', 'ab002000-0000-0000-0000-000000000002', 'Control',     '現行オンボーディング（5ステップ）', 50.00),
  ('a0000004-0001-0000-0000-000000000004', 'ab002000-0000-0000-0000-000000000002', 'Treatment A', '簡略化（3ステップ）',        50.00),
  ('a0000005-0001-0000-0000-000000000005', 'ab003000-0000-0000-0000-000000000003', 'Control',     '現行ダッシュボード',         33.33),
  ('a0000006-0001-0000-0000-000000000006', 'ab003000-0000-0000-0000-000000000003', 'Treatment A', 'カード型新デザイン',         33.33),
  ('a0000007-0001-0000-0000-000000000007', 'ab003000-0000-0000-0000-000000000003', 'Treatment B', 'リスト型新デザイン',         33.34),
  ('a0000008-0001-0000-0000-000000000008', 'ab004000-0000-0000-0000-000000000004', 'Control',     '朝8時通知',                  50.00),
  ('a0000009-0001-0000-0000-000000000009', 'ab004000-0000-0000-0000-000000000004', 'Treatment A', 'ユーザー習慣ベース通知',     50.00);

-- テスト結果（完了したテストのみ）
INSERT INTO ab_test_results (variant_id, metric_name, sample_size, metric_value, p_value, confidence_interval_lower, confidence_interval_upper, is_statistically_significant) VALUES
  ('a0000001-0001-0000-0000-000000000001', 'click_through_rate', 42000, 0.082500, NULL,     0.078,   0.087,   false),
  ('a0000002-0001-0000-0000-000000000002', 'click_through_rate', 43000, 0.097800, 0.00312,  0.093,   0.102,   true),
  ('a0000003-0001-0000-0000-000000000003', 'first_payment_conversion', 18500, 0.123000, NULL,    0.115,   0.131,   false),
  ('a0000004-0001-0000-0000-000000000004', 'first_payment_conversion', 18800, 0.158000, 0.00089,  0.149,   0.167,   true);

-- 勝者バリアントを設定
UPDATE ab_tests SET winner_variant_id = 'a0000002-0001-0000-0000-000000000002' WHERE id = 'ab001000-0000-0000-0000-000000000001';
UPDATE ab_tests SET winner_variant_id = 'a0000004-0001-0000-0000-000000000004' WHERE id = 'ab002000-0000-0000-0000-000000000002';

-- Auth Service Seed Data
-- 部門 UUID は App2 の departments テーブルと共有

INSERT INTO departments (id, name, code, description) VALUES
  ('de000001-0000-0000-0000-000000000001', 'SaaSプロダクト開発部',   'SAAS_DEV',       'BtoB SaaS製品の設計・開発'),
  ('de000002-0000-0000-0000-000000000002', 'ゲーム開発部',           'GAME_DEV',       'コンシューマー向けゲームの開発'),
  ('de000003-0000-0000-0000-000000000003', 'メディアプロダクト部',   'MEDIA_DEV',      'メディア・コンテンツサービスの開発'),
  ('de000004-0000-0000-0000-000000000004', 'フィンテック開発部',     'FINTECH_DEV',    'フィンテックサービスの開発'),
  ('de000005-0000-0000-0000-000000000005', 'ヘルスケア開発部',       'HEALTH_DEV',     'ヘルスケアサービスの開発'),
  ('de000006-0000-0000-0000-000000000006', 'QA・テスト部',           'QA',             '品質保証・テスト'),
  ('de000007-0000-0000-0000-000000000007', 'インフラ・SRE部',        'INFRA_SRE',      'インフラ・信頼性エンジニアリング'),
  ('de000008-0000-0000-0000-000000000008', 'デザイン部',             'DESIGN',         'UX/UIデザイン');

-- テストユーザー
-- パスワードはすべて "password123"
INSERT INTO users (id, username, email, password_hash, display_name, department_id, role) VALUES
  ('a0000001-0000-0000-0000-000000000001', 'admin',       'admin@example.com',        '$2b$11$P7MB2.jj7eKVvW4yzz2w4uy/ciGST1poC7SDe6PrfRqgrHQVYh/yO', 'システム管理者',     NULL,                                     'admin'),
  ('a0000002-0000-0000-0000-000000000002', 'saas_dev',    'saas_dev@example.com',     '$2b$11$P7MB2.jj7eKVvW4yzz2w4uy/ciGST1poC7SDe6PrfRqgrHQVYh/yO', 'SaaS開発 太郎',      'de000001-0000-0000-0000-000000000001',   'user'),
  ('a0000003-0000-0000-0000-000000000003', 'game_dev',    'game_dev@example.com',     '$2b$11$P7MB2.jj7eKVvW4yzz2w4uy/ciGST1poC7SDe6PrfRqgrHQVYh/yO', 'ゲーム開発 花子',    'de000002-0000-0000-0000-000000000002',   'user'),
  ('a0000004-0000-0000-0000-000000000004', 'media_dev',   'media_dev@example.com',    '$2b$11$P7MB2.jj7eKVvW4yzz2w4uy/ciGST1poC7SDe6PrfRqgrHQVYh/yO', 'メディア開発 次郎',  'de000003-0000-0000-0000-000000000003',   'user'),
  ('a0000005-0000-0000-0000-000000000005', 'fintech_dev', 'fintech_dev@example.com',  '$2b$11$P7MB2.jj7eKVvW4yzz2w4uy/ciGST1poC7SDe6PrfRqgrHQVYh/yO', 'フィンテック 三郎',  'de000004-0000-0000-0000-000000000004',   'user'),
  ('a0000006-0000-0000-0000-000000000006', 'health_dev',  'health_dev@example.com',   '$2b$11$P7MB2.jj7eKVvW4yzz2w4uy/ciGST1poC7SDe6PrfRqgrHQVYh/yO', 'ヘルスケア 四郎',    'de000005-0000-0000-0000-000000000005',   'user'),
  ('a0000007-0000-0000-0000-000000000007', 'qa_engineer', 'qa@example.com',           '$2b$11$P7MB2.jj7eKVvW4yzz2w4uy/ciGST1poC7SDe6PrfRqgrHQVYh/yO', 'QAエンジニア 五郎',  'de000006-0000-0000-0000-000000000006',   'user'),
  ('a0000008-0000-0000-0000-000000000008', 'sre_engineer','sre@example.com',          '$2b$11$P7MB2.jj7eKVvW4yzz2w4uy/ciGST1poC7SDe6PrfRqgrHQVYh/yO', 'SREエンジニア 六郎', 'de000007-0000-0000-0000-000000000007',   'user'),
  ('a0000009-0000-0000-0000-000000000009', 'designer',    'designer@example.com',     '$2b$11$P7MB2.jj7eKVvW4yzz2w4uy/ciGST1poC7SDe6PrfRqgrHQVYh/yO', 'デザイナー 七郎',    'de000008-0000-0000-0000-000000000008',   'user');

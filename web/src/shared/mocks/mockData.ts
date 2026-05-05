import type { Contract, Engineer, MatchCandidate, Project, Role, User } from '../types/domain'

export const users: Record<Role, User> = {
  Sales: { role: 'Sales', name: '佐藤 営業', email: 'sato@bridge.local' },
  Engineer: { role: 'Engineer', name: '田中 太郎', email: 'tanaka@bridge.local' },
}

export const projects: Project[] = [
  {
    id: 1,
    title: '金融系Webアプリ開発',
    client: 'A銀行',
    status: '募集中',
    owner: '佐藤 営業',
    period: '2025-06-01 〜 2025-12-31',
    startDate: '2025-06-01',
    price: '70万〜90万 / 月',
    summary: 'メガバンク向けの投信管理システム刷新。React と C# を用いた画面・API 開発を担当します。',
    assigned: '1 / 3名',
    matchScore: 85,
    requiredSkills: [
      { name: 'React', years: 3, type: '必須' },
      { name: 'TypeScript', years: 2, type: '必須' },
      { name: 'C#', years: 5, type: '必須' },
      { name: '金融ドメイン経験', years: 2, type: '必須' },
    ],
    welcomeSkills: [{ name: 'AWS', years: 1, type: '歓迎' }],
  },
  {
    id: 2,
    title: 'SaaSフロントエンド開発',
    client: 'B社',
    status: '募集中',
    owner: '佐藤 営業',
    period: '2025-06-01 〜 2026-03-31',
    startDate: '2025-06-01',
    price: '80万〜100万 / 月',
    summary: 'エンタープライズSaaSの管理画面刷新。コンポーネント設計と状態管理の改善が中心です。',
    assigned: '0 / 2名',
    matchScore: 92,
    requiredSkills: [
      { name: 'React', years: 4, type: '必須' },
      { name: 'TypeScript', years: 3, type: '必須' },
      { name: '設計レビュー', years: 2, type: '必須' },
    ],
    welcomeSkills: [{ name: 'AWS', years: 1, type: '歓迎' }],
  },
  {
    id: 3,
    title: '新規Webサービス立ち上げ',
    client: 'C社',
    status: '募集中',
    owner: '佐藤 営業',
    period: '2025-05-15 〜 2025-11-30',
    startDate: '2025-05-15',
    price: '75万〜95万 / 月',
    summary: 'Next.js を使った新規サービスの初期開発。要件整理からリリースまで小規模チームで進めます。',
    assigned: '1 / 2名',
    matchScore: 87,
    requiredSkills: [
      { name: 'TypeScript', years: 3, type: '必須' },
      { name: 'Next.js', years: 2, type: '必須' },
    ],
    welcomeSkills: [{ name: 'UI設計', years: 1, type: '歓迎' }],
  },
  {
    id: 4,
    title: 'モダン基幹システム',
    client: 'D社',
    status: 'クローズ',
    owner: '佐藤 営業',
    period: '2025-07-01 〜 2026-06-30',
    startDate: '2025-07-01',
    price: '70万〜90万 / 月',
    summary: '既存基幹システムを段階的に Web 化。C# と React の両方を扱うポジションです。',
    assigned: '2 / 2名',
    matchScore: 81,
    requiredSkills: [
      { name: 'C#', years: 4, type: '必須' },
      { name: 'React', years: 2, type: '必須' },
    ],
    welcomeSkills: [{ name: 'Azure', years: 1, type: '歓迎' }],
  },
]

export const engineers: Engineer[] = [
  {
    id: 1,
    name: '田中 太郎',
    status: '稼働中',
    project: '金融系Webアプリ開発',
    availableFrom: '2025-06-01',
    sales: '佐藤 営業',
    unitPrice: '85万',
    skills: ['React 5年', 'TypeScript 4年', 'C# 3年', 'AWS 2年'],
    categories: ['Web開発', 'フロントエンド'],
    avoid: ['パッケージ導入', '運用保守'],
  },
  {
    id: 2,
    name: '鈴木 花子',
    status: '稼働中',
    project: '新規Webサービス立ち上げ',
    availableFrom: '2025-05-01',
    sales: '佐藤 営業',
    unitPrice: '82万',
    skills: ['TypeScript 5年', 'Next.js 3年', 'Node.js 4年'],
    categories: ['Web開発', 'バックエンド'],
    avoid: ['常駐運用'],
  },
  {
    id: 3,
    name: '山田 次郎',
    status: '空き',
    project: '-',
    availableFrom: '2025-05-01',
    sales: '佐藤 営業',
    unitPrice: '78万',
    skills: ['C# 6年', 'React 2年', 'Azure 3年'],
    categories: ['業務システム', 'クラウド'],
    avoid: ['短期案件'],
  },
]

export const matchCandidates: MatchCandidate[] = [
  {
    rank: 1,
    engineer: engineers[0],
    score: 85,
    breakdown: { skill: 55, years: 20, preference: 10 },
    evaluations: [
      { skill: 'React', type: '必須', requiredYears: 3, actualYears: 5, status: 'matched' },
      { skill: 'TypeScript', type: '必須', requiredYears: 2, actualYears: 4, status: 'matched' },
      { skill: 'C#', type: '必須', requiredYears: 5, actualYears: 3, status: 'insufficient' },
      { skill: '金融ドメイン経験', type: '必須', requiredYears: 2, actualYears: null, status: 'unmet' },
      { skill: 'AWS', type: '歓迎', requiredYears: 1, actualYears: 2, status: 'matched' },
    ],
  },
  {
    rank: 2,
    engineer: engineers[1],
    score: 72,
    breakdown: { skill: 45, years: 17, preference: 10 },
    evaluations: [
      { skill: 'React', type: '必須', requiredYears: 3, actualYears: null, status: 'unmet' },
      { skill: 'TypeScript', type: '必須', requiredYears: 2, actualYears: 5, status: 'matched' },
      { skill: 'C#', type: '必須', requiredYears: 5, actualYears: null, status: 'unmet' },
      { skill: 'AWS', type: '歓迎', requiredYears: 1, actualYears: null, status: 'unmet' },
    ],
  },
]

export const contracts: Contract[] = [
  { title: '金融系Webアプリ開発(A銀行)', period: '2025-03-01 〜 2025-05-31', unitPrice: '85万', type: '更新', current: true },
  { title: '金融系Webアプリ開発(A銀行)', period: '2024-12-01 〜 2025-02-28', unitPrice: '85万', type: '初回', current: false },
  { title: 'パッケージ導入支援(Z社)', period: '2024-06-01 〜 2024-11-30', unitPrice: '75万', type: '更新', current: false },
  { title: 'パッケージ導入支援(Z社)', period: '2024-03-01 〜 2024-05-31', unitPrice: '70万', type: '初回', current: false },
]

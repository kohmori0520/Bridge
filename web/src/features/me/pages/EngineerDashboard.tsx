import { Edit, MailOutlined, Search, WarningAmber } from '@mui/icons-material'
import { Alert, Button, Stack } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import { Info } from '../../../shared/components/Info'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { MatchProjectRow } from '../../matching/components/MatchProjectRow'
import { useMyProjectMatches } from '../../projects/hooks/useProjects'

export function EngineerDashboard() {
  const { data: matchedProjects = [] } = useMyProjectMatches()

  return (
    <Stack spacing={2}>
      <PageHeader title="エンジニアダッシュボード" subtitle="契約状況と次の案件候補" />
      <Section title="契約ステータス">
        <Alert severity="warning" icon={<WarningAmber />} action={<Button startIcon={<MailOutlined />}>連絡する</Button>}>
          要注意: 契約終了まで残り 23 日・更新未定
        </Alert>
        <div className="detail-grid">
          <Info label="現在の案件" value="金融系Webアプリ開発(A銀行)" />
          <Info label="契約期間" value="2025-03-01 〜 2025-05-31" />
          <Info label="単価" value="85万円 / 月" />
          <Info label="担当営業" value="佐藤 営業" />
        </div>
      </Section>
      <Section title="キャリア志向" action={<Button component={RouterLink} to="/me/profile" endIcon={<Edit />}>編集する</Button>}>
        <div className="detail-grid">
          <Info label="希望スキル" value="React, TypeScript, AWS" />
          <Info label="希望カテゴリ" value="Web開発, フロントエンド" />
          <Info label="避けたい業務" value="パッケージ導入, 運用保守" />
        </div>
      </Section>
      <Section title="あなたにマッチする案件(トップ3)" action={<Button component={RouterLink} to="/me/matches" endIcon={<Search />}>すべて</Button>}>
        <Stack spacing={1}>
          {matchedProjects
            .slice(0, 3)
            .map((project, index) => (
              <MatchProjectRow key={project.id} rank={index + 1} project={project} />
            ))}
        </Stack>
      </Section>
    </Stack>
  )
}

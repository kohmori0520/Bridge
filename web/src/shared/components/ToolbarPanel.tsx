import { Search } from '@mui/icons-material'
import { Button, FormControl, InputLabel, MenuItem, Paper, Select, TextField } from '@mui/material'

export function ToolbarPanel() {
  return (
    <Paper className="toolbar-panel" variant="outlined">
      <TextField size="small" label="キーワード" placeholder="氏名・案件名・スキル" />
      <FormControl size="small">
        <InputLabel id="status-filter-label">ステータス</InputLabel>
        <Select labelId="status-filter-label" label="ステータス" defaultValue="all">
          <MenuItem value="all">すべて</MenuItem>
          <MenuItem value="open">募集中</MenuItem>
          <MenuItem value="active">稼働中</MenuItem>
          <MenuItem value="available">空き</MenuItem>
        </Select>
      </FormControl>
      <Button startIcon={<Search />} variant="contained">
        検索
      </Button>
    </Paper>
  )
}

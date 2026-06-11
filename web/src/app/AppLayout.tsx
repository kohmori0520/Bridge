import { Dashboard, Groups, History, Logout, ManageAccounts, Person, Search, Work } from '@mui/icons-material'
import { AppBar, Box, Button, Chip, Container, Tab, Tabs, Toolbar, Typography } from '@mui/material'
import { Link as RouterLink, Navigate, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { useNotify } from '../shared/notifications/useNotify'

export function AppLayout() {
  const { user, logout } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const notify = useNotify()

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  const tabs =
    user.role === 'Admin'
      ? [
          { label: 'ホーム', icon: <Dashboard />, to: '/' },
          { label: 'ユーザー作成', icon: <ManageAccounts />, to: '/admin/users/new' },
          { label: '更新間近契約', icon: <History />, to: '/admin/contracts/expiring' },
          { label: '技術者', icon: <Groups />, to: '/engineers' },
        ]
      : user.role === 'Sales'
        ? [
            { label: 'ホーム', icon: <Dashboard />, to: '/' },
            { label: '案件', icon: <Work />, to: '/projects' },
            { label: '技術者', icon: <Groups />, to: '/engineers' },
          ]
        : [
            { label: 'ホーム', icon: <Dashboard />, to: '/' },
            { label: 'プロフィール', icon: <Person />, to: '/me/profile' },
            { label: '契約履歴', icon: <History />, to: '/me/contracts' },
            { label: '公開案件', icon: <Work />, to: '/projects' },
            { label: 'マッチ', icon: <Search />, to: '/me/matches' },
          ]
  const currentTab = tabs.findIndex((tab) =>
    tab.to === '/' ? location.pathname === '/' : location.pathname.startsWith(tab.to),
  )

  return (
    <Box className="app-shell">
      <AppBar position="sticky" elevation={0}>
        <Toolbar className="topbar">
          <Typography variant="h6" component={RouterLink} to="/" className="brand">
            Bridge
          </Typography>
          <Box className="user-area">
            <Chip size="small" color={user.role === 'Admin' ? 'warning' : user.role === 'Sales' ? 'primary' : 'success'} label={user.role} />
            <Typography variant="body2">{user.name} さん</Typography>
            <Button
              color="inherit"
              startIcon={<Logout />}
              onClick={async () => {
                await logout()
                notify('ログアウトしました', 'info')
                navigate('/login')
              }}
            >
              ログアウト
            </Button>
          </Box>
        </Toolbar>
        <Tabs
          value={currentTab === -1 ? false : currentTab}
          className="nav-tabs"
          textColor="inherit"
          indicatorColor="secondary"
          variant="scrollable"
          scrollButtons="auto"
        >
          {tabs.map((tab) => (
            <Tab key={tab.to} icon={tab.icon} iconPosition="start" label={tab.label} component={RouterLink} to={tab.to} />
          ))}
        </Tabs>
      </AppBar>
      <Container maxWidth="xl" className="page-container">
        <Outlet />
      </Container>
    </Box>
  )
}

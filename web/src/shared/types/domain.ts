export type Role = 'Sales' | 'Engineer'

export type User = {
  role: Role
  name: string
  email: string
}

export type LoginCredentials = {
  email: string
  password: string
}

export type RequiredSkill = {
  skillId?: number
  name: string
  years: number
  type: '必須' | '歓迎'
}

export type Project = {
  id: number
  title: string
  client: string
  status: '下書き' | '募集中' | 'クローズ'
  owner: string
  period: string
  startDate: string
  price: string
  summary: string
  assigned: string
  requiredSkills: RequiredSkill[]
  welcomeSkills: RequiredSkill[]
  matchScore: number
}

export type Engineer = {
  id: number
  name: string
  status: '稼働中' | '空き'
  project: string
  availableFrom: string
  sales: string
  unitPrice: string
  skills: string[]
  categories: string[]
  avoid: string[]
}

export type MatchCandidate = {
  rank: number
  engineer: Engineer
  score: number
  breakdown: {
    skill: number
    years: number
    preference: number
  }
  evaluations: {
    skill: string
    type: '必須' | '歓迎'
    requiredYears: number
    actualYears: number | null
    status: 'matched' | 'insufficient' | 'unmet'
  }[]
}

export type Contract = {
  title: string
  period: string
  unitPrice: string
  type: '初回' | '更新'
  current: boolean
}

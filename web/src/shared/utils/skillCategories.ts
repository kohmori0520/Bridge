export const skillCategoryOrder = ['Language', 'Framework', 'Infrastructure', 'Database', 'Domain', 'Role', 'ProductType', 'Tool', 'Other']

export const skillCategoryLabels: Record<string, string> = {
  Language: '言語',
  Framework: 'フレームワーク',
  Infrastructure: 'インフラ',
  Database: 'データベース',
  Domain: '業務ドメイン',
  Role: '役割',
  ProductType: 'プロダクト種別',
  Tool: 'ツール',
  Other: 'その他',
}

export function getSkillCategoryLabel(category: string) {
  return skillCategoryLabels[category] ?? category
}

export function sortSkillCategories(categories: string[]) {
  return categories.slice().sort((a, b) => {
    const aIndex = skillCategoryOrder.indexOf(a)
    const bIndex = skillCategoryOrder.indexOf(b)
    return (aIndex === -1 ? Number.MAX_SAFE_INTEGER : aIndex) - (bIndex === -1 ? Number.MAX_SAFE_INTEGER : bIndex)
  })
}

export function groupBySkillCategory<T extends { category: string }>(items: T[]) {
  return items.reduce<Record<string, T[]>>((groups, item) => {
    groups[item.category] = [...(groups[item.category] ?? []), item]
    return groups
  }, {})
}

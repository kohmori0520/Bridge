import { describe, expect, it } from 'vitest'
import { getSkillCategoryLabel, groupBySkillCategory, sortSkillCategories } from './skillCategories'

describe('getSkillCategoryLabel', () => {
  it('定義済みカテゴリは日本語ラベルを返す', () => {
    expect(getSkillCategoryLabel('Language')).toBe('言語')
    expect(getSkillCategoryLabel('Framework')).toBe('フレームワーク')
  })

  it('未定義カテゴリはそのまま返す', () => {
    expect(getSkillCategoryLabel('Unknown')).toBe('Unknown')
  })
})

describe('sortSkillCategories', () => {
  it('定義された表示順でソートする', () => {
    expect(sortSkillCategories(['Tool', 'Language', 'Database', 'Framework'])).toEqual([
      'Language',
      'Framework',
      'Database',
      'Tool',
    ])
  })

  it('未知のカテゴリは末尾に回す', () => {
    expect(sortSkillCategories(['Unknown', 'Language'])).toEqual(['Language', 'Unknown'])
  })

  it('元の配列を破壊しない', () => {
    const input = ['Tool', 'Language']
    sortSkillCategories(input)
    expect(input).toEqual(['Tool', 'Language'])
  })
})

describe('groupBySkillCategory', () => {
  it('カテゴリごとにグループ化する', () => {
    const items = [
      { name: 'React', category: 'Framework' },
      { name: 'TypeScript', category: 'Language' },
      { name: 'Vue', category: 'Framework' },
    ]

    expect(groupBySkillCategory(items)).toEqual({
      Framework: [
        { name: 'React', category: 'Framework' },
        { name: 'Vue', category: 'Framework' },
      ],
      Language: [{ name: 'TypeScript', category: 'Language' }],
    })
  })

  it('空配列は空オブジェクトを返す', () => {
    expect(groupBySkillCategory([])).toEqual({})
  })
})

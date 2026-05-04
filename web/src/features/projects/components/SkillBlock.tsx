import { Box, Chip, Stack, Typography } from '@mui/material'
import type { RequiredSkill } from '../../../shared/types/domain'

type SkillBlockProps = {
  title: string
  skills: RequiredSkill[]
}

export function SkillBlock({ title, skills }: SkillBlockProps) {
  return (
    <Box className="info-item wide">
      <Typography variant="caption" color="text.secondary">
        {title}
      </Typography>
      <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}>
        {skills.map((skill) => (
          <Chip
            key={skill.name}
            size="small"
            color={skill.type === '必須' ? 'primary' : 'default'}
            label={`${skill.name} (${skill.years}年以上)`}
          />
        ))}
      </Stack>
    </Box>
  )
}

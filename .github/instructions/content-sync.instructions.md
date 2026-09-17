---
applyTo: "README*.md,docs/localization*.md"
---

# Content Synchronization

Keep each source document synchronized with all of its localized variants.

## Synchronized Documents

| Source document        | Localized filename pattern                   | Current localized documents  |
| ---------------------- | -------------------------------------------- | ---------------------------- |
| `README.md`            | `README.<BCP-47-language-tag>.md`            | `README.zh-TW.md`            |
| `docs/localization.md` | `docs/localization.<BCP-47-language-tag>.md` | `docs/localization.zh-TW.md` |

## Synchronization Rules

- Treat the source document as the canonical version.
- When a source document changes, update every existing localized variant in the same change.
- Use a valid BCP 47 language tag immediately before the `.md` extension, with conventional casing such as `zh-TW`.
- Keep each localized document as a complete translation of its source document.
- Preserve heading structure, code blocks, command examples, links, file paths, technical identifiers, and behavior descriptions unless localization requires a small wording adjustment.
- If content is added, removed, renamed, or reordered in a source document, mirror the same change in every localized variant before finishing the task.
- When directly editing a localized document, verify that it remains structurally and semantically aligned with its source document.
- Add new source documents and their localized filename patterns to the table when they become part of the synchronized documentation set.

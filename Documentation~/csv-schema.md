# CSV Schema Version 1

Dreamy Localization uses UTF-8 CSV. Records follow standard CSV quoting:

- Fields containing commas, quotes, carriage returns, or newlines are quoted.
- Quotes inside quoted fields are doubled.
- Export uses LF newlines and deterministic table/key ordering.

## String CSV

```csv
schema,table,key,id,source,en,vi,status,smart,shared_comment,en_comment,vi_comment,tags,context
1,UI,home.title,42,Home,Home,Trang chu,approved,false,Home title,,,ui,Main menu
```

Required columns:

- `schema`
- `table`
- `key`
- At least one locale column such as `en` or `vi`

Reserved columns:

- `id`
- `source`
- `status`
- `smart`
- `asset_type`
- `preload`
- `shared_comment`
- `<locale>_comment`
- `tags`
- `context`

Supported statuses:

- `draft`
- `translated`
- `review`
- `approved`
- `deprecated`
- `ignore`

Rows marked `ignore` or `deprecated` may omit translations. Other rows require
a value for each locale column.

## Asset CSV

Asset rows use the same document model and add `asset_type` and `preload`.
Locale values contain project-relative asset paths or GUID references. Asset
resolution and Asset Table import are implemented in the import phase.

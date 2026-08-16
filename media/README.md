# Public media

- `hub-thumbnail.png`: square source-controlled thumbnail for the COI Hub listing.
- `social-preview.png`: wide social image for the GitHub repository.

Both files are generated from original Recursive Industry icon exports by
`tools/generate_public_media.ps1`. They contain no game assets.

Regenerate on Windows with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/generate_public_media.ps1
```

Real in-game screenshots should accompany the stable Hub release; these graphics
are identity assets, not substitutes for gameplay evidence.

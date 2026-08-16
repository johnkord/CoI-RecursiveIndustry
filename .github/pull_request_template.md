## Summary

Describe the player-visible or contributor-facing change.

## Validation

- [ ] `python tools/validate_public_repo.py`
- [ ] `python -m unittest discover -s tests -p "test_*.py"`
- [ ] Release build completed locally when C# changed
- [ ] In-game validation described when behavior or presentation changed

## Compatibility and policy

- [ ] Stable prototype IDs are preserved or migration impact is documented
- [ ] No game DLLs, unmodified game assets, logs, saves, or credentials are included
- [ ] New work can be contributed under COI-Open and the Modding Policy
- [ ] `CHANGELOG.md` and relevant public docs are updated

## Tradeoff

For gameplay changes, state the material, power, maintenance, validation, labor,
logistics, or opportunity cost and when the conventional route should still win.

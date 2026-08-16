# Transported product models

Recursive Industry's eight Foundation products share one original shallow
clipped-corner cartridge mesh. Product-specific albedos preserve identity while a
shared normal and smooth-metal map keep the family visually coherent.

Canonical production assets are under
`production/v1-cartridge-family/assets/`:

- one normalized OBJ mesh used for LOD 0 through 4;
- eight product-specific albedos;
- shared normal and smooth-metal textures; and
- a deterministic review sheet under `proofs/`.

`asset-manifest.json` binds the source hashes, runtime Unity paths, topology, and
dependency-free `cartridge_c874` bundle.

The mesh inherits each source product's game size, packing mode, and storage-rack
orientation. UI icons and transported-product graphics are independent contracts;
changing one does not automatically change the other.

These original mod assets are covered by COI-Open and are not standalone asset
packs for use outside Captain of Industry.

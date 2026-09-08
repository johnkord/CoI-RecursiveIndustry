# Civic Knowledge

Civic Knowledge is an optional settlement investment introduced in 0.25.0a and
retained in 0.28.0a with an independent research path.
It does not replace housing, clinics, food, sanitation, or the player's decisions.

## Supply the Service

One Civic Model Center uses 40 workers, 1 MW, 128 Computing, and Maintenance III.
Every 360 Time, its recipe consumes one Model Archive, eight Dataset Archives,
one Validated Research Dossier, and eight Office Supplies to produce 1,600
Civic Knowledge Stream.

Connect that stream over its own Fiber line to a Knowledge Commons attached to
the settlement. The Commons uses 12 workers, 250 kW, and Maintenance II. It
consumes 0.2 Stream per person per month and buffers 240 Stream. Full satisfaction
contributes 1.2 Unity; there is no Health or worker-productivity effect.

The stream is a distinct non-storable Data product. It is not Industrial Control
Stream, and different services cannot share a mixed-product link. The Gateway
is not a substitute for the Civic Model Center.

## Population and Capacity

| Boundary | Capacity |
| --- | ---: |
| One center | 266 2/3 Stream per 60 Time, approximately 1,333 people |
| One Access link | 200 Stream per 60 Time, 1,000 people |
| One Backbone link | 450 Stream per 60 Time, 2,250 people with enough centers |

For 1,000 people, the center operates at 75% of its declared capacity. Its direct
inputs per 3,600 simulation Time are 7.5 Models, 60 Datasets, 7.5 Dossiers, and
60 Office Supplies. The Commons buffer lasts 1.2 months at that demand, before
considering Fiber contents. A cut or a shortage eventually removes this service's
bonus; it does not disable existing services or housing.

These are capacity and demand calculations, not guarantees of actual network
flow, priority, or uptime. Source batches, delivery, connected population, and
other consumers still matter.

## Share Existing Institutions

At the 1,000-person reference demand, combined civic and physical-science support
needs 9.375 Models and 225 Datasets per 3,600 Time. At default source rates one
Model Development Center, one Curation Office, one Pilot Science Complex, and
one AI Science Institute can supply that demand. Existing spare production can
serve it; none of those buildings must be duplicated merely because the service
is new.

Starting all of that from scratch is a substantial investment: 268 workers
including the Center and one Commons, and 224 peak Computing before other island
demands. External laboratory inputs, Titanium, Office Supplies, maintenance,
coolant, power generation, and the Data Center itself remain necessary. The
service is intended for an established island with capacity it wants to invest.

The research follows Fiber Infrastructure, Physical Validation, and ISP Module,
with four lifetime Dossiers and sixteen lifetime Models. It is optional for all
other applications and does not require a Planetary Center, Industrial Control
Gateway, or Microchip consolidation.

## Sources

- [Civic declarations](../mods/RecursiveIndustry/src/CivicKnowledgeData.cs).
- [Compact contract](../data/civic-knowledge.json).
- [Capacity and shared-support model](../tools/model_civic_knowledge.py).
- Native Calendar: one month equals 60 simulation Time on the verified target.
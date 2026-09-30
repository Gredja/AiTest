---
case: Meridian Retail Group — Click & Collect
feature: Web reservation → in-store pickup within 48h → POS QR scan → loyalty credit
date: 2026-09-30
author: Alexey
---

# Test plan — Click & Collect (Phase 1)

## In scope

- **Web cart and reservation step** — customer reserves an item on meridian.com and gets a 48-hour pickup window
- **Identity stitch** — customer's existing in-store loyalty account merging with the web account on first pickup
- **SAP-sourced inventory check at pickup time** — freshness and read behaviour of the SAP-synced stock at pickup confirmation
- **Cross-region loyalty-points credit** — points for the purchase credited correctly across regions after pickup
- **POS pickup confirmation** — QR scan at the store POS completes the handover

## Out of scope

- **SAP ECC inventory ground-truth correctness** — owned by Finance, covered by their own controls; we test how the platform reads SAP data at pickup, not whether SAP's numbers are right.
- **Legacy Shopify storefronts** — being strangled away in Phase 1; no new test investment in something that is being switched off.

## Top-3 risks if the in-scope work fails

1. **Phantom-stock cancellation at pickup.** A stale SAP read lets the web reserve an item the store no longer has, so the POS cancels the order when the customer arrives. The customer leaves empty-handed with no refund initiated — trust in Click & Collect is gone for that customer. Meridian eats direct revenue loss on every occurrence and the documented 7% cancellation baseline climbs — David Park's store-ops worst case.
2. **Identity stitch merges two different customers.** A collision links one person's web account to another person's loyalty history. The wrong customer sees another customer's tier, points, and purchase history — a personal-data disclosure at the POS. That is a GDPR incident on Asha Sundaram's escalation path, with regulatory notification attached.
3. **PSD2 SCA failure on EU pickup confirmation.** The SCA challenge fails or drops at the confirmation step for EU payment methods. The EU customer cannot complete pickup and the reservation is cancelled after travelling to the store. Drop-off concentrates in exactly the markets the pilot is meant to prove — Marco Rossi's Italy rollout is the first casualty.

## Entry criteria

1. Phase 1 build deployed to the QA region.
2. SAP sandbox seeded with realistic inventory deltas, including staleness scenarios.
3. Identity-provider stub configured for merged and duplicate accounts.

## Exit criteria

- Pass rate ≥ 95% on critical-path test cases.
- Zero phantom-stock cancellations on priority-1 test cases.
- Named sign-off from David Park (Head of Retail Ops) and Sarah Chen (Head of CX).

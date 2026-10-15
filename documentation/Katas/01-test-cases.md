---
case: Meridian Retail Group — Click & Collect
consumes: 00-test-plan.md (Kata 6.W.1)
date: 2026-09-30
author: Alexey
---

# 01-test-cases.md — Click & Collect test suite

**In scope reference (from 00-test-plan.md):**

1. Web cart and reservation step
2. Identity stitch (web account ↔ in-store loyalty account)
3. SAP-sourced inventory check at pickup time
4. Cross-region loyalty-points credit
5. POS pickup confirmation

---

## Seeds (5, one sentence each)

1. Happy reservation and pickup in Milano.
2. Cross-region reservation — Italian customer collects at a German store.
3. Identity stitch on first in-store pickup after web sign-up.
4. Loyalty-points credit for a partial pickup.
5. SAP inventory freshness check at pickup confirmation.

---

## Seed 1 — Happy reservation and pickup in Milano

| ID | Test case | Type | Category | Priority | In-scope item |
|----|-----------|------|----------|----------|---------------|
| T01 | Italian customer reserves a kettle on meridian.com for Milano Duomo, collects within 24h: POS scans QR, SAP confirms stock <30s stale, item released, points credit | happy | critical-path | 1 | Web cart/reservation; POS confirmation |
| T02 | Pickup at 47h59 of the 48h window (Milano, Postepay order) still completes end to end | edge | edge | 2 | Web cart/reservation |
| T03 | POS scans a reservation already cancelled as a past-48h no-show — handover rejected, no item released | negative | regression | 2 | POS confirmation |

## Seed 2 — Cross-region reservation (Italian customer → German store)

| ID | Test case | Type | Category | Priority | In-scope item |
|----|-----------|------|----------|----------|---------------|
| T04 | Italian customer reserves on the IT site and collects at a Berlin store in German — journey completes with correct store attribution | happy | edge | 2 | Web cart/reservation; POS confirmation |
| T05 | Cross-region pickup receipt and points display in the pickup country's currency with the customer's home loyalty tier intact | edge | edge | 3 | Cross-region loyalty-points credit |
| T06 | Customer in a region where Click & Collect is not live yet tries to reserve — clear rejection, no half-created order | negative | regression | 3 | Web cart/reservation |

## Seed 3 — Identity stitch on first pickup after web sign-up

| ID | Test case | Type | Category | Priority | In-scope item |
|----|-----------|------|----------|----------|---------------|
| T07 | First in-store pickup after web sign-up merges the web account with the existing loyalty account — one history, prior points intact | happy | critical-path | 1 | Identity stitch |
| T08 | Customer holding 3–4 fragmented regional loyalty accounts resolves to one deterministic winner after stitch | edge | edge | 2 | Identity stitch |
| T09 | Loyalty number that resolves to two different customer records must NOT merge — reject and escalate, no cross-customer history visible | negative | edge | 1 | Identity stitch |

## Seed 4 — Loyalty-points credit for partial pickup

| ID | Test case | Type | Category | Priority | In-scope item |
|----|-----------|------|----------|----------|---------------|
| T10 | Full pickup → points visible in the customer's app within 30s of POS confirmation | happy | critical-path | 2 | Cross-region loyalty-points credit |
| T11 | Multi-item order with one line unavailable at the store: available line released and credited, missing line cancelled with customer notice | edge | edge | 2 | Loyalty credit; POS confirmation |
| T12 | POS confirms pickup but the points credit call fails — customer keeps the item, system retries and alerts, points never silently lost | negative | critical-path | 2 | Cross-region loyalty-points credit |

## Seed 5 — SAP inventory freshness at pickup confirmation

| ID | Test case | Type | Category | Priority | In-scope item |
|----|-----------|------|----------|----------|---------------|
| T13 | SAP read at confirmation is ≤30s stale → POS confirms handover and the order completes | happy | critical-path | 1 | SAP-sourced inventory check |
| T14 | Two customers hold the last unit of the same SKU in different stores and arrive in the same window — neither store double-releases | edge | edge | 2 | SAP-sourced inventory check |
| T15 | SAP returns zero stock at pickup (reserved-away after booking) — handover safely rejected with refund/alternative path, never a phantom release | negative | critical-path | 1 | SAP-sourced inventory check |

## Risk-coverage additions (gaps found against 00-test-plan risks 2–3)

| ID | Test case | Type | Category | Priority | In-scope item |
|----|-----------|------|----------|----------|---------------|
| T16 | PSD2 SCA challenge fails at EU pickup confirmation (Postepay) — reservation held 10 minutes with retry, no silent cancel | negative | critical-path | 1 | Web cart/reservation |
| T17 | SAP inventory service times out at confirmation — deterministic fallback to the reservation snapshot, no indefinite hang | negative | critical-path | 2 | SAP-sourced inventory check |

## Smoke coverage gap

| ID | Test case | Type | Category | Priority | In-scope item |
|----|-----------|------|----------|----------|---------------|
| T18 | Click & Collect reservation page loads for an in-stock Milano SKU and shows availability plus the 48h window | happy | smoke | 4 | Web cart/reservation |

---

## Suite stats

- **Total: 18 cases** (within the 15–20 target)
- **Negatives: 7** — T03, T06, T09, T12, T15, T16, T17 (floor of 5: met)
- **Categories:** critical-path 8 · edge 7 · regression 2 · smoke 1
- **Priorities:** P1 6 (T01, T07, T09, T13, T15, T16) · P2 9 · P3 2 · P4 1

## Re-tag and priority notes (steps 5–7)

- **Dedup:** "partial pickup" appeared from both the loyalty and SAP seeds — merged into T11 (loyalty outcome) and T15 (stock outcome); kept one wording each.
- **Re-tagged from AI proposal:** cross-region journey (T04) and identity-merge collision (T09) proposed as critical-path → **edge** (kata definition: uncommon but consequential); T03 and T06 proposed as critical-path → **regression** (standard rejection behaviour).
- **Priority vs plan risks:** P1 reserved for what blocks the Italy pilot — happy path (T01), both GDPR identity outcomes (T07, T09), SAP freshness and phantom stock (T13, T15), PSD2 SCA (T16 = plan risk 3). T17 kept P2: fallback behaviour matters, but the safe-reject case (T15) is the pilot blocker.

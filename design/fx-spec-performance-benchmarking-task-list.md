# FX.Spec Performance Benchmarking – Implementation Backlog

This backlog translates the design document into actionable engineering tasks. Each task includes a brief description, primary outputs, and suggested dependency notes. Sequence numbers reflect recommended execution order but tasks can be parallelized when dependencies allow.

## Legend

- **Type**: Feature (F), Infrastructure (I), Documentation (D), Verification (V).
- **Deps**: Key prerequisites before starting the task.
- **Deliverables**: Concrete outputs to target before closing the task.

## Phase 1 – Foundation

| ID | Type | Title | Summary | Deliverables | Deps | Status |
|----|------|-------|---------|--------------|------|--------|
| T1 | I | Module scaffolding | Create `FX.Spec.Performance` project, include in solution, wire into CI build matrix (Release focus). | New project files, solution update, baseline CI build passing. | — | ✅ Complete |
| T2 | I | Runner extension points | Expose plugin hook enabling perf suites, ensure CLI guard prevents non-Release runs, tag registration. | Runner extension code, tests covering tag filtering & Release validation. | T1 | 🚧 In progress |

## Phase 2 – DSL & Authoring Surface

| ID | Type | Title | Summary | Deliverables | Deps | Status |
|----|------|-------|---------|--------------|------|--------|
| T3 | F | DSL primitives | Implement `describePerf`, `itPerf`, `PerfSpecBuilder`, async measurement pipe, tagging integration. | DSL APIs, sample spec, unit tests. | T1, T2 | ✅ Complete (scaffold) |
| T4 | V | Runner integration smoke tests | Author sample perf suite; validate discovery, inclusion via `--tags perf`, opt-out when module absent. | Integration tests, sample spec project. | T3 | ⏳ Pending |

## Phase 3 – Measurement Engines

| ID | Type | Title | Summary | Deliverables | Deps | Status |
|----|------|-------|---------|--------------|------|--------|
| T5 | F | Microbenchmark harness | Stopwatch-based measurement with warmup/iteration control, GC+allocation metrics, unit wrappers. | Measurement engine, helper APIs, accuracy tests. | T3 | ⏳ Pending |
| T6 | F | Load-test measurement contract | Define interfaces/DTOs for load results, placeholder backend for matchers. | Contract types, mock backend, matcher stubs. | T3 | ⏳ Pending |

## Phase 4 – Profiles & Baselines

| ID | Type | Title | Summary | Deliverables | Deps | Status |
|----|------|-------|---------|--------------|------|--------|
| T7 | F | Profile schema & loader | JSON schema, validation, merge precedence (env → local → repo → built-in). | Schema docs, loader code, validation tests. | T3 | ⏳ Pending |
| T8 | I | Profile storage helpers | File-system helpers for local (`~/.fx-spec/perf/profiles`) and repo (`perf-profiles/`) paths; cross-platform support. | Path utilities, IO tests. | T7 | ⏳ Pending |

## Phase 5 – Calibration Workflow

| ID | Type | Title | Summary | Deliverables | Deps | Status |
|----|------|-------|---------|--------------|------|--------|
| T9 | F | `perf calibrate` CLI command | Route CLI command, Release guard, discover-only execution mode. | CLI handler, smoke tests. | T3, T7 | ⏳ Pending |
| T10 | F | Hardware fingerprinting | Capture CPU, core count, memory across OS targets; integrate with calibration output. | Hardware info module, unit tests per platform. | T9 | ⏳ Pending |
| T11 | V | Calibration output management | Support `--overwrite`, `--append`, optional env override for output dir. | CLI tests, documentation snippet. | T9 | ⏳ Pending |

## Phase 6 – Reporting Enhancements

| ID | Type | Title | Summary | Deliverables | Deps | Status |
|----|------|-------|---------|--------------|------|--------|
| T12 | F | Reporter performance section | Add “Performance Summary” with pass/fail status, near-threshold warnings, baseline provenance. | Reporter updates, golden tests. | T3, T5, T6, T7 | ⏳ Pending |
| T13 | F | JSON performance report | Implement `--perf-report json`, documented schema. | JSON exporter, serialization tests. | T12 | ⏳ Pending |

## Phase 7 – Overrides & Configuration

| ID | Type | Title | Summary | Deliverables | Deps | Status |
|----|------|-------|---------|--------------|------|--------|
| T14 | F | Environment override parser | Parse absolute/relative override env vars, default tolerance handling. | Parser utilities, unit tests. | T7 | ⏳ Pending |
| T15 | V | Reporter override visibility | Ensure overrides flagged in output, messaging tests. | Reporter fixtures, doc updates. | T12, T14 | ⏳ Pending |

## Phase 8 – Load Testing Backend

| ID | Type | Title | Summary | Deliverables | Deps | Status |
|----|------|-------|---------|--------------|------|--------|
| T16 | I | `wrk` discovery & runner | Lookup `wrk`, honor env overrides, actionable errors, skip option. | Process wrapper, discovery tests. | T6 | ⏳ Pending |
| T17 | F | CLI argument builder & parser | Translate DSL config to wrk args; parse stdout for metrics. | Arg builder, parser tests with fixtures. | T16 | ⏳ Pending |
| T18 | F | Load-test matchers | Implement throughput, latency percentile, error rate matchers with tolerance support. | Matcher implementations, unit tests. | T17, T14 | ⏳ Pending |
| T19 | V | Failure handling & resilience | Add timeouts, cancellation, retries/noise dampening hooks, verbose streaming. | Resilience logic, integration tests. | T17 | ⏳ Pending |

## Phase 9 – Documentation & Adoption

| ID | Type | Title | Summary | Deliverables | Deps | Status |
|----|------|-------|---------|--------------|------|--------|
| T20 | D | Documentation suite | Guides: getting started, calibration workflow, CI integration, wrk troubleshooting. | Docs in `docs/`, examples. | T3–T19 (rolling) | ⏳ Pending |
| T21 | F | Update FX gate scripts | Refactor `scripts/check-gates.sh` to call perf suites, maintain fallback path. | Updated scripts, release notes. | T12–T19 | ⏳ Pending |
| T22 | F | Templates & samples | Provide template perf spec project showcasing micro & load tests, overrides, calibration. | Sample projects, README updates. | T3–T19 | ⏳ Pending |

## Phase 10 – Stabilization & Rollout

| ID | Type | Title | Summary | Deliverables | Deps | Status |
|----|------|-------|---------|--------------|------|--------|
| T23 | V | Internal pilot & CI shadowing | Run new perf suites alongside existing gates for multiple cycles, capture deltas. | CI reports, issue backlog. | T12–T22 | ⏳ Pending |
| T24 | V | Feedback & tuning loop | Gather feedback, adjust tolerances, document mitigation strategies, track open questions. | Retro notes, tuning backlog. | T23 | ⏳ Pending |
| T25 | D | Release preparation | Version bump, changelog entries, upgrade checklist (wrk install, calibration), communication plan. | Release notes, comms template. | T24 | ⏳ Pending |

## Cross-Cutting Notes

- Treat profiling integration with BenchmarkDotNet, hardware fingerprint fidelity, tolerance defaults, and alternative load generator support as tracked open questions; create follow-up design spikes as needed.
- Ensure every task includes corresponding automated tests or validation scripts to maintain Release-only coverage confidence.
- Coordinate with CI/CD stakeholders early to provision dedicated hardware and wrk availability; include them in pilot sign-off.
- Maintain a running risk log for measurement noise, profile drift, and developer ergonomics; review at the close of each phase.
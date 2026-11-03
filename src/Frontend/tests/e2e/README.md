# E2E tests

## Run

Install the pinned browser once with `npx playwright install chromium`, then run the suite with:

```sh
npm run test:e2e
```

<<<<<<< HEAD
Playwright starts a local mock API on port 5100 and the Next.js development server on port 3100. It reuses existing servers on those ports during local development. Search, sitemap, and robots tests run as required checks and fail if the corresponding route or behavior is missing.
=======
Playwright starts a local mock API and the Next.js development server on port 3000. It reuses an existing server on that port during local development. The search flow tests exercise the browser form, query-string navigation, result rendering, and empty-result suggestions when the 4.13 search screen is present; they report as skipped while the route is still a placeholder. The sitemap/robots tests work with either the local mock API or the seeded development API.
>>>>>>> ab3c2e293ec2f852ad8e33644415acff05ba5cec

## Add a module flow

- Put each flow in `tests/e2e/<module>.spec.ts` and use accessible roles and labels for selectors.
- Keep API fixtures in the mock server when the test is checking frontend behavior. Use an integration environment when the test must cover the real API/database contract.
- For authentication, have TV1 supply a dedicated test account or a test-only login fixture; do not commit credentials or reuse a personal account.
- For recipe creation and publishing, have TV2 supply a cleanup-safe test recipe fixture. Delete or restore created data in `finally`/fixture teardown so reruns are repeatable.
- Keep assertions on user-visible outcomes and important URL changes. Avoid selectors tied to styling classes.

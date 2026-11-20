import http from 'k6/http';
import { check, sleep } from 'k6';

const apiBase = (__ENV.API_BASE_URL || 'http://localhost/api/v1').replace(/\/$/, '');

export const options = {
  scenarios: {
    public_recipe_reads: {
      executor: 'constant-vus',
      vus: 50,
      duration: '5m',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<500'],
  },
};

export function setup() {
  const response = http.get(`${apiBase}/recipes?page=1&pageSize=1`);
  check(response, { 'recipe list is available': (res) => res.status === 200 });
  const body = response.json();
  const slug = body?.items?.[0]?.slug;
  if (!slug) throw new Error('GET /recipes did not return a recipe slug');
  return { slug };
}

export default function ({ slug }) {
  const list = http.get(`${apiBase}/recipes?page=1&pageSize=12`);
  check(list, { 'recipe list returns 200': (res) => res.status === 200 });

  const detail = http.get(`${apiBase}/recipes/${encodeURIComponent(slug)}`);
  check(detail, { 'recipe detail returns 200': (res) => res.status === 200 });

  const search = http.get(`${apiBase}/recipes/search?q=pho%20bo`);
  check(search, { 'recipe search returns 200': (res) => res.status === 200 });
  sleep(0.1);
}

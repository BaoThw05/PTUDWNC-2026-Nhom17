import { chromium } from "../../src/Frontend/node_modules/playwright/index.mjs";
import { readFile, writeFile } from "node:fs/promises";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage();
const response = await page.goto("https://search.google.com/test/rich-results", {
  waitUntil: "domcontentloaded",
  timeout: 60_000,
});
await page.waitForTimeout(5_000);
await page.mouse.click(470, 295);
await page.waitForTimeout(1_000);
const html = await readFile("docs/evidence/tv4/recipe-pho-bo-rich-results.html", "utf8");
await page.locator("textarea:visible").first().fill(html);
await page.screenshot({ path: "docs/evidence/tv4/rich-results-code-input.png", fullPage: true });
const preSubmit = await page.locator("body").innerText();
const testButton = page.getByText(/test code/i).first();
if (await testButton.count()) {
  await testButton.click({ timeout: 5_000 });
  await page.waitForTimeout(20_000);
}
const result = await page.evaluate(() => ({
  title: document.title,
  body: document.body.innerText.slice(0, 8_000),
  buttons: [...document.querySelectorAll("button")].map((button) => button.innerText.trim()).filter(Boolean),
  roleButtons: [...document.querySelectorAll('[role="button"],mwc-button')].map((button) => ({
    text: button.innerText.trim(),
    ariaLabel: button.getAttribute("aria-label"),
    disabled: button.getAttribute("aria-disabled") ?? button.hasAttribute("disabled"),
  })),
  inputs: [...document.querySelectorAll("input,textarea,[contenteditable=true]")].map((input) => ({
    tag: input.tagName,
    placeholder: input.getAttribute("placeholder"),
    ariaLabel: input.getAttribute("aria-label"),
    role: input.getAttribute("role"),
    visible: Boolean(input.getClientRects().length),
  })),
}));
const evidence = { status: response?.status() ?? null, preSubmit: preSubmit.slice(0, 2_000), ...result };
await page.screenshot({ path: "docs/evidence/tv4/rich-results-code-input.png", fullPage: true });
await writeFile("docs/evidence/tv4/rich-results-code-input.json", JSON.stringify(evidence, null, 2));
console.log(JSON.stringify(evidence, null, 2));
await browser.close();

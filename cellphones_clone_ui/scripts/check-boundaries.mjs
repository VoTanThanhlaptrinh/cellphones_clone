// Kiểm tra ranh giới phụ thuộc giữa các thư mục trong src/app (xem ADMIN_MERGE_PLAN.md).
//
//   core   -> chỉ core
//   shared -> shared và core/models (không inject service)
//   store  -> store, shared, core   (không admin)
//   admin  -> admin, shared, core   (không store)
//
// Chạy: npm run check:boundaries
import { readdirSync, readFileSync, statSync } from 'node:fs';
import { dirname, join, relative, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const appDir = resolve(dirname(fileURLToPath(import.meta.url)), '..', 'src', 'app');

/** Layer nào đã được dọn xong thì mới kiểm tra; thêm vào đây khi hoàn thành bước tương ứng. */
const ENFORCED = new Set(['core', 'shared', 'store', 'admin']);

const RULES = {
  core: { allow: ['core'] },
  shared: { allow: ['shared', 'core/models'] },
  store: { allow: ['store', 'shared', 'core'] },
  admin: { allow: ['admin', 'shared', 'core'] },
};

const layerOf = (absPath) => {
  const first = relative(appDir, absPath).split(sep)[0];
  return first in RULES ? first : null;
};

/** Đổi import (alias hoặc tương đối) thành đường dẫn tương đối so với src/app, hoặc null nếu là package ngoài. */
function resolveTarget(fromFile, spec) {
  const alias = /^@(core|shared|store|admin)\/(.*)$/.exec(spec);
  if (alias) return `${alias[1]}/${alias[2]}`;
  if (spec.startsWith('.')) return relative(appDir, resolve(dirname(fromFile), spec)).split(sep).join('/');
  return null;
}

function* walk(dir) {
  for (const name of readdirSync(dir)) {
    const full = join(dir, name);
    if (statSync(full).isDirectory()) yield* walk(full);
    else if (name.endsWith('.ts') && !name.endsWith('.spec.ts')) yield full;
  }
}

const importRe = /(?:from\s+|import\s*\(\s*|import\s+)['"]([^'"]+)['"]/g;
const violations = [];

for (const file of walk(appDir)) {
  const layer = layerOf(file);
  if (!layer || !ENFORCED.has(layer)) continue;
  const text = readFileSync(file, 'utf8');
  for (const [, spec] of text.matchAll(importRe)) {
    const target = resolveTarget(file, spec);
    if (target === null || target.startsWith('..')) continue; // package ngoài hoặc file ngoài src/app (environments)
    const ok = RULES[layer].allow.some((prefix) => target === prefix || target.startsWith(prefix + '/'));
    if (!ok) violations.push(`${relative(appDir, file)}: '${spec}' (${layer} không được import ${target.split('/')[0]})`);
  }
}

if (violations.length) {
  console.error(`Vi phạm ranh giới import (${violations.length}):\n- ` + violations.join('\n- '));
  process.exit(1);
}
console.log(`OK: không vi phạm ranh giới import (kiểm tra: ${[...ENFORCED].join(', ')}).`);

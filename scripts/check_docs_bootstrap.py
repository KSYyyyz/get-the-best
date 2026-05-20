#!/usr/bin/env python3
"""校验 Get The Best 新仓库初始化文档。"""

from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
DOCS = ROOT / "docs"

REQUIRED_FILES = [
    ROOT / "README.md",
    DOCS / "README.md",
    DOCS / "get_the_best_v2_execution_index.md",
    DOCS / "get_the_best_v2_reference_game_study.md",
    DOCS / "get_the_best_v2_engine_plugin_strategy.md",
    DOCS / "get_the_best_v2_reset_architecture.md",
]

REQUIRED_TERMS = [
    "Get The Best",
    "壮志凌云",
    "现金流可支撑时间",
    "文档正文",
    "中文",
]

ENGLISH_TEMPLATE_MARKERS = [
    "Implementation Plan",
    "Goal:",
    "Architecture:",
    "Tech Stack:",
    "Task 1:",
    "Step 1:",
    "Expected:",
    "Self-Review",
]


def main() -> int:
    failures: list[str] = []
    texts: list[str] = []

    for path in REQUIRED_FILES:
        if not path.exists():
            failures.append(f"缺少必需文件: {path.relative_to(ROOT)}")
            continue
        try:
            text = path.read_text(encoding="utf-8")
        except UnicodeDecodeError as exc:
            failures.append(f"文件不是有效 UTF-8: {path.relative_to(ROOT)}: {exc}")
            continue
        if "\x00" in text:
            failures.append(f"文件包含 NUL 字节: {path.relative_to(ROOT)}")
        texts.append(text)

    markdown_files = sorted(ROOT.rglob("*.md"))
    all_markdown = "\n".join(path.read_text(encoding="utf-8") for path in markdown_files)
    combined = "\n".join(texts)
    for term in REQUIRED_TERMS:
        if term not in combined:
            failures.append(f"缺少必需词: {term}")

    for marker in ENGLISH_TEMPLATE_MARKERS:
        if marker in all_markdown:
            failures.append(f"发现英文模板残留: {marker}")

    if failures:
        print("Get The Best 文档初始化检查失败:")
        for failure in failures:
            print(f"  - {failure}")
        return 1

    print("Get The Best 文档初始化检查通过。")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

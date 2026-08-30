# Markdown 全面指南：表格、数学公式、饼图与流程图

Markdown 是一种轻量级标记语言，广泛用于文档编写、技术博客、笔记和项目说明。它易于读写，且能转换为 HTML、PDF 等多种格式。本文将重点介绍 Markdown 中的**表格**、**数学公式**（LaTeX）、**饼图**（Mermaid）和**流程图**（Mermaid）等高级特性。

---

## 1. 基础语法速览

- **标题**：`# H1`, `## H2`, `### H3` ...
- **粗体**：`**粗体**`
- **斜体**：`*斜体*`
- **链接**：`[文字](url)`
- **图片**：`![替代文字](图片链接)`
- **代码块**：三个反引号 + 语言名

---

## 2. 表格（原生支持）

Markdown 使用竖线 `|` 和短横 `-` 创建表格，第二行用于对齐。

| 功能     | 语法示例              | 是否支持 |
|----------|-----------------------|----------|
| 左对齐   | `:---`                | ✅       |
| 居中对齐 | `:---:`               | ✅       |
| 右对齐   | `---:`                | ✅       |
| 合并单元格 | 不支持原生，需HTML  | ❌       |

**示例表格（带对齐）：**

| 姓名     | 年龄 | 职业     |
|:---------|:----:|---------:|
| 张三     | 28   | 工程师   |
| 李四     | 32   | 设计师   |
| 王五     | 24   | 产品经理 |

> 表格内容支持粗体、斜体、链接，但不支持代码块或列表（可嵌套HTML解决）。

---

## 3. 数学公式（LaTeX）

通过 `$...$` 行内公式和 `$$...$$` 块级公式，配合 MathJax 或 KaTeX 渲染。

**行内公式**：质能方程 \( E = mc^2 \)。

**块级公式**：
$$
\int_{-\infty}^{\infty} e^{-x^2} dx = \sqrt{\pi}
$$

**矩阵示例**：
$$
\begin{bmatrix}
a & b \\
c & d
\end{bmatrix}
\cdot
\begin{bmatrix}
x \\
y
\end{bmatrix}
=
\begin{bmatrix}
ax + by \\
cx + dy
\end{bmatrix}
$$

**多行对齐**：
$$
\begin{aligned}
\nabla \times \vec{B} &= \mu_0 \vec{J} + \mu_0 \epsilon_0 \frac{\partial \vec{E}}{\partial t} \\
\nabla \cdot \vec{E} &= \frac{\rho}{\epsilon_0}
\end{aligned}
$$

---

## 4. 饼图（Mermaid）

Mermaid 是 Markdown 中流行的图表绘制工具，支持饼图、流程图、序列图等。在代码块中指定 `mermaid` 语言。

```mermaid
pie title 编程语言使用比例（示例）
    "Python" : 45
    "JavaScript" : 30
    "Java" : 15
    "C++" : 10
# System Guidelines

## General Rules

- Always generate clean, modular React code
- Prefer composition of components over custom implementations
- Do not create custom UI components if a library component exists
- Keep layout responsive using standard layout primitives
- Do not use inline styles unless strictly necessary

---

# Design System Guidelines — Ant Design (STRICT MODE)

## Core Rule (NON-NEGOTIABLE)

- ALL UI must be built using official Ant Design (`antd`) components
- NEVER create custom components that replicate Ant Design components
- NEVER override, extend, or modify Ant Design components
- NEVER customize styles or components directly
- ONLY customization allowed is through official Ant Design theme tokens (ConfigProvider)
- NEVER use custom CSS to change Ant Design behavior or appearance
- ALWAYS use default Ant Design props and behavior

If a component exists in Ant Design, it MUST be used exactly as provided.

---

## Imports (MANDATORY)

- Always import components from `antd`
- Always import styles using `import 'antd/dist/reset.css';`
- Use `@ant-design/icons` for icons when needed
- NEVER use external icon libraries

---

## Layout

- Use ONLY Ant Design layout components:
  - `Layout`
  - `Header`
  - `Sider`
  - `Content`
  - `Footer`
- Use Ant Design Grid system:
  - `Row`
  - `Col`
- NEVER use custom div-based layout when an AntD layout component exists
- NEVER create custom spacing systems
- NEVER create custom wrappers for layout styling
- NEVER alter spacing, padding, margins, widths, heights, borders, shadows, or alignment to simulate a custom design language

---

## Components Usage (MANDATORY)

Always use official Ant Design components for:

- Buttons → `Button`
- Forms → `Form`, `Input`, `InputNumber`, `Select`, `Checkbox`, `Radio`, `DatePicker`, `TimePicker`, `Switch`
- Tables → `Table`
- Modals → `Modal`
- Feedback → `Alert`, `message`, `notification`, `Spin`, `Result`
- Navigation → `Menu`, `Tabs`, `Breadcrumb`, `Pagination`, `Dropdown`
- Data Display → `Card`, `List`, `Descriptions`, `Tag`, `Badge`, `Tooltip`
- Overlay → `Drawer`, `Popover`
- Actions → `Popconfirm`
- Layout helpers → `Space`, `Divider`, `Flex` when available in Ant Design

If Ant Design already has a component for the use case, it MUST be used.

---

## Styling (STRICTLY FORBIDDEN)

Do NOT:

- Add custom CSS
- Override Ant Design classes
- Use `styled-components`
- Use Tailwind
- Use CSS Modules
- Use Sass or Less for visual overrides
- Change colors
- Change typography
- Change border radius
- Change spacing
- Change shadows
- Change component sizes outside the official props exposed by Ant Design
- Apply inline styles to modify visual appearance
- Create a custom theme
- Use design tokens different from the default Ant Design tokens
- Use `ConfigProvider` to customize theme values
- Add custom backgrounds, containers, panels, wrappers, or decorative blocks that change the native Ant Design look

The UI must remain visually identical to default Ant Design.

---

## Behavior

- Use default component behavior only
- Do not implement custom interactions if Ant Design already provides them
- Use built-in validation from Ant Design `Form`
- Use built-in pagination, sorting, and filtering from Ant Design `Table`
- Use Ant Design interaction patterns exactly as provided
- Do not replace native Ant Design flows with custom logic for the sake of visual customization

---

## Anti-Patterns (FORBIDDEN)

- Creating custom buttons instead of `Button`
- Styling components manually
- Recreating layout with `div` instead of `Layout`
- Mixing other UI libraries such as Material UI, Bootstrap, Chakra, Radix, shadcn, or custom component kits
- Using custom typography instead of Ant Design defaults
- Wrapping Ant Design components in custom components that alter their appearance
- Rebuilding cards, tables, forms, sidebars, headers, modals, or menus manually
- Using absolute positioning to imitate a custom design
- Using custom icons when an Ant Design icon exists
- Generating a branded interface
- Generating a “modernized” or “enhanced” version of Ant Design
- Making the UI “prettier” by deviating from the original Ant Design appearance

---

## Enforcement Rule

If Ant Design provides a solution:
- USE IT EXACTLY AS IS

If Ant Design does NOT provide a solution:
- Use the closest possible Ant Design component
- DO NOT invent visual patterns
- DO NOT create a custom design language
- Prefer functional simplicity over custom visuals

---

## Output Expectation

- The result must look like a native Ant Design application
- No visual deviations
- No branding
- No custom UI layer
- No style overrides
- No theme overrides
- Pure Ant Design only

---

## Failure Condition

The generation is invalid if ANY of the following happens:

- Any custom styling is introduced
- Any Ant Design component is visually modified
- Any layout is created outside official Ant Design layout primitives when an official component exists
- Any other UI library is used
- Any custom design system pattern is introduced
- The final result does not look like a default Ant Design application

If there is any conflict between usability preferences and Ant Design defaults, Ant Design defaults must win.

---

## Final Rule

In ALL cases, at ALL times, use 100% original Ant Design components with NO customization whatsoever.

NEVER customize ANYTHING.

This rule overrides all other stylistic decisions.
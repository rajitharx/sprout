# Accessibility & WCAG AA Compliance Guide

This document outlines the accessibility improvements made to the Sprout project to achieve WCAG 2.1 AA compliance.

## Overview

Sprout now includes comprehensive accessibility features to ensure both the child view and parent panel are usable by all users, including those with disabilities.

## Key Accessibility Features

### 1. Semantic HTML

- **Header/Main/Footer Elements**: Used semantic tags (`<header>`, `<main>`, `<footer>`, `<nav>`, `<section>`, `<article>`) to provide proper document structure
- **Heading Hierarchy**: Proper heading levels (`<h1>`, `<h2>`) used throughout
- **Form Labels**: All form inputs have associated `<label>` elements with proper `htmlFor` attributes
- **Lists**: Task indicators use semantic role attributes for proper structure

### 2. ARIA Labels & Attributes

#### Interactive Elements
- All buttons have descriptive `aria-label` attributes
- Dynamic content uses `aria-live="polite"` for status updates
- Error messages use `role="alert"` with `aria-live="assertive"`
- Form inputs have `aria-required="true"` for required fields

#### Navigation & Structure
- Task carousel marked as region with `role="region"`
- Task indicators use `role="tablist"` and `role="tab"` semantics
- Weekly progress bar uses `role="progressbar"` with `aria-valuenow`, `aria-valuemin`, `aria-valuemax`
- Dialogs use `role="dialog"` with `aria-modal="true"`

#### Descriptions
- SVG icons marked with `aria-hidden="true"` to prevent screen reader clutter
- Emoji indicators properly labeled with `aria-label`
- Progress information announced with `aria-label` and live regions

### 3. Keyboard Navigation

#### Parent Panel
- Tab navigation through all interactive elements
- Arrow keys in task carousel: Left/Right to navigate between tasks
- Enter key to submit forms
- Proper tab order maintained throughout

#### Child View
- Left/Right arrow keys to navigate task carousel
- Tab key to access navigation buttons
- Enter/Space to activate buttons
- Settings button accessible via tab key

### 4. Focus Management

#### Visual Focus Indicators
- 3px solid orange (#fb923c) outline on all focusable elements
- 2px outline-offset for better visibility
- High contrast mode support with enhanced borders
- Focus visible only on keyboard navigation (not mouse click)

#### Focus Styling
```css
button:focus-visible,
input:focus-visible,
[role="button"]:focus-visible {
  outline: 3px solid #fb923c;
  outline-offset: 2px;
}
```

### 5. Color Contrast

**WCAG AA Compliance** (4.5:1 for normal text, 3:1 for large text)

- Dark text on light backgrounds: ✓ Compliant
- White text on orange backgrounds: ✓ Compliant
- Error messages (red text on red background): Updated to use text color with sufficient contrast
- All button states reviewed for contrast

### 6. Reduced Motion Support

Respects `prefers-reduced-motion` media query:

```css
@media (prefers-reduced-motion: reduce) {
  /* All animations disabled */
}
```

This affects:
- Floating animations
- Celebration effects
- Transitions
- Loading states

Users can enable this in OS accessibility settings, and all animations will be disabled.

### 7. Screen Reader Support

#### Invisible Text (`.sr-only`)
Used for:
- Instructions ("Click or press Enter to continue")
- Status updates
- Completion indicators

#### Live Regions
- Toast notifications: `aria-live="polite"` for non-urgent updates
- Alert banners: `aria-live="assertive"` for urgent messages
- Progress updates: Announced dynamically

#### Hidden Content
- Decorative emojis and icons: `aria-hidden="true"`
- Purely visual elements excluded from screen reader navigation

### 8. Form Accessibility

#### Labels & Required Fields
- All inputs have associated labels
- Required fields marked with `aria-required="true"` and visual indicator
- Error messages associated via `aria-describedby`

#### Input Types
- PIN input uses `inputMode="numeric"` for better mobile UX
- Password field properly typed as "password"
- All placeholders supplement, not replace, labels

#### Validation
- Error messages announced to screen readers with `role="alert"`
- Field validation uses `aria-invalid="true"` when applicable

### 9. Modal & Dialog Accessibility

#### Structure
- Modals use `role="dialog"` with `aria-modal="true"`
- Dialog title associated with `aria-labelledby`
- Dialog description associated with `aria-describedby`

#### Focus Management
- Focus moves to modal on appearance
- Focus trapped within modal
- Focus returns to trigger element when modal closes

### 10. Icon & Image Accessibility

#### SVG Icons
- Decorative icons: `aria-hidden="true"`
- Functional icons: Parent button has descriptive label
- No icon-only buttons without labels

#### Emojis
- Emoji indicators: `aria-hidden="true"` for visual emojis with text labels nearby
- Emoji as content: Included in accessible name with `aria-label`

## Testing & Compliance

### Automated Tools
- TypeScript strict mode enforces type safety
- ESLint for accessibility rules (via tailwind plugins)
- Color contrast validation

### Manual Testing
- Keyboard navigation: Tab through entire interface
- Screen reader: Test with VoiceOver, NVDA, or JAWS
- Zoom: Test at 200% zoom level
- High contrast mode: Verify visibility
- Reduced motion: Verify animations disable

### Supported Assistive Technologies
- Screen readers: NVDA, JAWS, VoiceOver
- Magnification: Browser zoom, OS-level zoom
- Keyboard-only navigation: Full support
- Voice control: Compatible with standard commands

## Component-Specific Details

### StreakBar
- Weekly progress displayed as navigation region
- Each day indicator has clear label
- Status (completed/today/upcoming/not completed) announced

### TaskCarousel
- Carousel implemented as region with keyboard support
- Arrow keys navigate between tasks
- Current task and progress clearly indicated
- Swipe and pointer events supplemented by keyboard

### TaskCard
- Task presented as article with proper heading
- Emoji decoration marked as non-semantic
- Completion status announced to screen readers

### DoneButton
- Large touch target (96px minimum height)
- Clear state indication (different colors/text)
- Ripple effect respects reduced motion preference
- Star animation only on completion, respects prefers-reduced-motion

### ParentPanel
- Full keyboard navigation support
- Drag-and-drop reordering supplemented by accessible labels
- Form validation with error announcements
- Tab navigation through task list

### PinAuthModal
- Password input properly typed
- Error messages announced
- Enter key submits form
- Numeric input mode for PIN entry

## Browser & Platform Support

### Browsers
- Chrome 90+ (Chromium)
- Firefox 88+
- Safari 14+
- Edge 90+

### Platforms
- Windows (NVDA, JAWS)
- macOS (VoiceOver)
- iOS (VoiceOver)
- Android (TalkBack)

## Future Improvements

1. **Localization**: Add language-specific accessibility features
2. **Custom Focus Indicators**: Allow user customization of focus appearance
3. **Animation Preferences**: More granular control over specific animations
4. **High Contrast Mode**: OS-level high contrast support detection
5. **Text Scaling**: Ensure readability at different text sizes
6. **Voice Input**: Support for voice commands (beyond browser defaults)

## Resources

- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [ARIA Authoring Practices](https://www.w3.org/WAI/ARIA/apg/)
- [WebAIM](https://webaim.org/)
- [MDN Accessibility](https://developer.mozilla.org/en-US/docs/Web/Accessibility)

## Contributing

When adding new features, ensure:
1. Semantic HTML is used for structure
2. All interactive elements are keyboard accessible
3. Focus indicators are visible and clear
4. ARIA labels/descriptions are provided where needed
5. Color contrast meets WCAG AA standards
6. Animations respect `prefers-reduced-motion`
7. Screen reader compatibility is tested

## Questions?

For accessibility questions or issues, refer to this document or create an issue in the repository with the accessibility label.

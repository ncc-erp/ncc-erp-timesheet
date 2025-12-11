import { Component, EventEmitter, Input, Output } from '@angular/core';

export type ButtonColor = 'primary' | 'secondary' | 'danger' | 'default';
export type ButtonVariant = 'solid' | 'outline' | 'text';
export type ButtonSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-button',
  templateUrl: './button.component.html',
  styleUrls: ['./button.component.css']
})
export class ButtonComponent {
  @Input() color: ButtonColor = 'primary';
  @Input() variant: ButtonVariant = 'solid';
  @Input() size: ButtonSize = 'md';
  @Input() disabled = false;
  @Input() loading = false;
  @Input() icon: string;
  @Input() iconPosition: 'left' | 'right' = 'left';
  @Input() type: 'button' | 'submit' = 'button';

  @Output() clicked = new EventEmitter<MouseEvent>();

  onClick(event: MouseEvent): void {
    if (this.disabled || this.loading) {
      event.preventDefault();
      return;
    }
    this.clicked.emit(event);
  }

  get hostClasses(): string[] {
    return [
      `btn--color-${this.color}`,
      `btn--variant-${this.variant}`,
      `btn--size-${this.size}`,
      this.loading ? 'btn--loading' : ''
    ].filter(Boolean);
  }
}

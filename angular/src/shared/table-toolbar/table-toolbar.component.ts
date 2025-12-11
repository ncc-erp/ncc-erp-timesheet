import { Component, EventEmitter, Input, Output } from '@angular/core';

export interface TableToolbarFilterOption {
  value: any;
  label: string;
}

export interface TableToolbarFilterConfig {
  key: string;
  placeholder?: string;
  options: TableToolbarFilterOption[];
}

export interface TableToolbarActionConfig {
  key: string;
  label: string;
  icon?: string;
  color?: 'primary' | 'secondary' | 'danger' | 'default';
}

@Component({
  selector: 'app-table-toolbar',
  templateUrl: './table-toolbar.component.html',
  styleUrls: ['./table-toolbar.component.css']
})
export class TableToolbarComponent {
  @Input() title: string;
  @Input() filters: TableToolbarFilterConfig[] = [];
  @Input() searchPlaceholder = 'Search';
  @Input() showSearch = true;
  @Input() searchText = '';
  @Input() actions: TableToolbarActionConfig[] = [];

  @Output() filterChange = new EventEmitter<{ key: string; value: any }>();
  @Output() searchChange = new EventEmitter<string>();
  @Output() actionClick = new EventEmitter<string>();

  onFilterChange(key: string, value: any): void {
    this.filterChange.emit({ key, value });
  }

  onSearchChange(value: string): void {
    this.searchText = value;
    this.searchChange.emit(this.searchText);
  }

  onActionClick(key: string): void {
    this.actionClick.emit(key);
  }
}

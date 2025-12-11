import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-pagination-bar',
  templateUrl: './pagination-bar.component.html',
  styleUrls: ['./pagination-bar.component.css']
})
export class PaginationBarComponent {
  @Input() pageIndex = 0;
  @Input() length = 0;
  @Input() pageSize = 10;
  @Input() pageSizeOptions: number[] = [10, 20, 50];

  @Output() pageChange = new EventEmitter<{ pageIndex: number; pageSize: number }>();

  onPageEvent(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.pageChange.emit({ pageIndex: this.pageIndex, pageSize: this.pageSize });
  }
}

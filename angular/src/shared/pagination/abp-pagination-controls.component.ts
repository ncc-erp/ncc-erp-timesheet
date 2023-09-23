import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, ViewEncapsulation } from '@angular/core';

@Component({
    selector: 'abp-pagination-controls',
    templateUrl: './abp-pagination-controls.component.html',
    styleUrls: ['./abp-pagination.css']
})
export class AbpPaginationControlsComponent {
    @Input() id: string;
    @Input() maxSize = 7;

    @Input()
    get directionLinks(): boolean {
        return this._directionLinks;
    }

    set directionLinks(value: boolean) {
        this._directionLinks = !!value && <any>value !== 'false';
    }

    @Input()
    get autoHide(): boolean {
        return this._autoHide;
    }

    set autoHide(value: boolean) {
        this._autoHide = !!value && <any>value !== 'false';
    }
    @Input() previousLabel = 'Previous';
    @Input() nextLabel = 'Next';
    @Input() screenReaderPaginationLabel = 'Pagination';
    @Input() screenReaderPageLabel = 'page';
    @Input() screenReaderCurrentLabel = `You're on page`;
    @Output() pageChange: EventEmitter<number> = new EventEmitter<number>();
    @Output() selectionChange: EventEmitter<number> = new EventEmitter<number>();
    @Input() totalNumber = 0;

    private _directionLinks = true;
    private _autoHide = false;
    listSelections = [5, 10, 15, 20, 25, 30, 50, 100,200,300];
    selection = 10;

    change(): void {
        this.selectionChange.emit(this.selection);
    }
}

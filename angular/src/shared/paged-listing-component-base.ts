import { AppComponentBase } from 'shared/app-component-base';
import { Injector, OnInit, OnDestroy  } from '@angular/core';
import { SortDirectionEnum } from './AppEnums';

export class PagedResultDto {
    items: any[];
    totalCount: number;
}

export class EntityDto {
    id: number;
}

export class FilterDto{
    propertyName: string;
    value: any;
    comparison: number;
    //comparisionName: string;
}

export class PagedRequestDto {
    skipCount: number;
    maxResultCount: number;
    searchText: string;
    filterItems: FilterDto[] = [];
    sort: string;
    sortDirection: number;
}
export abstract class PagedListingComponentBase<TEntityDto> extends AppComponentBase implements OnInit, OnDestroy  {

    public pageSize = 10;
    public pageNumber = 1;
    public totalPages = 1;
    public totalItems: number;
    public isTableLoading = false;
    public searchText: string = '';
    public filterItems: FilterDto[] = [];
    public sortProperty: string = "";
    public sortDirection: number = null
    public sortDirectionEnum = SortDirectionEnum
    constructor(injector: Injector) {
        super(injector);
    }

    ngOnInit(): void {
        this.refresh();
    }

    public searchOrFilter(){
        this.pageNumber = 1;
        this.refresh();
    }

    public getPageSize(value): void {
        this.pageSize = value;
        this.refresh();
    }

    refresh(): void {
        this.getDataPage(this.pageNumber);
    }

    public showPaging(result: PagedResultDto, pageNumber: number): void {
        this.totalPages = ((result.totalCount - (result.totalCount % this.pageSize)) / this.pageSize) + 1;

        this.totalItems = result.totalCount;
        this.pageNumber = pageNumber;
    }

    public getDataPage(page: number): void {
        var req = new PagedRequestDto();
        req.maxResultCount = this.pageSize;
        req.skipCount = (page - 1) * this.pageSize;
        req.searchText = this.searchText;
        req.filterItems = this.filterItems;

        if (this.sortProperty) {
            req.sort = this.sortProperty;
            req.sortDirection = this.sortDirection;
        }

        this.isTableLoading = true;
        this.list(req, page, () => {
            this.isTableLoading = false;
        });
    }


    public onAddedFilterItem(item: FilterDto){
        // console.log('onAddedFilterItem()');
        // console.log(item);
        if (this.filterItems.findIndex(i => i.propertyName == item.propertyName && i.comparison == item.comparison) < 0){
            this.filterItems.push(item);
        }
        // console.log(this.filterItems);
    }
    

    public deleteFilterItem(item: FilterDto){
        var index = this.filterItems.findIndex(i => i.comparison==item.comparison && i.propertyName==item.propertyName && i.value == item.value);
        if (index >= 0) this.filterItems.splice(index, 1);
    }
    public onSortChange(property: string) {
        if (this.sortProperty != property) {
            this.sortDirection = null
        }
        console.log(this.sortDirection)
        if (property) {
            switch (this.sortDirection) {
                case null: {
                    this.sortDirection = this.sortDirectionEnum.Ascending
                    this.sortProperty = property
                    break;
                }
                case this.sortDirectionEnum.Ascending: {
                    this.sortDirection = this.sortDirectionEnum.Descending
                    this.sortProperty = property
                    break;
                }
                case this.sortDirectionEnum.Descending: {
                    this.sortDirection = null
                    this.sortProperty = ""
                    break;
                }
            }
        }
        this.refresh()
    }
    protected abstract list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void;
    protected abstract delete(entity: TEntityDto): void;
    ngOnDestroy(){
        this.subscriptions.forEach(sub => {
            sub.unsubscribe()
        })
    }
}

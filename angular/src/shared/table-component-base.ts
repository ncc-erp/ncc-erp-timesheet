import { AppComponentBase } from 'shared/app-component-base';
import { Injector, OnInit } from '@angular/core';
import { PaginationDTO } from './pagination/pagination-DTO';
import { FormBuilder, Validators } from '@angular/forms';
import { FilterRequest } from '@app/service/api/model/common-DTO';
import { Observable } from 'rxjs';

export abstract class TableComponentBase extends AppComponentBase {
  searchParam: any = {};
  data: any = [];
  pageDto : PaginationDTO = new PaginationDTO();
  isTableLoading: boolean = false;
  
  constructor(
    injector: Injector
  ) {
    super(injector);
  }

  buildData(data: Array<any>) {
    return data;
  }

  search(){
    this.isTableLoading = true;
    this.searchParam = this.buildParam();
    this.searchParam.page = this.pageDto.currentPage + 1;
    this.searchParam.pageSize = this.pageDto.pageSize;
    this.searchApi(this.searchParam).subscribe(res => {
      this.isTableLoading = false;
      this.pageDto.totalItems = res['result'].totalItems;
      this.data = this.buildData(res['result'].items);
      console.log(this.data);
    })
  }

  pageChange(e:any){
    this.isTableLoading = true;
    this.pageDto.currentPage = e.pageIndex;
    this.search();
  }

  abstract buildParam(): FilterRequest;
  abstract searchApi(param: FilterRequest): Observable<any>;
}

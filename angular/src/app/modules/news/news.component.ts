import { Component, OnInit, Injector } from '@angular/core';
import { MatDialog } from '@angular/material';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { ManageNewsService } from '@app/service/api/manage-new.service';
import { NewsDto } from '@app/service/api/model/news-dto';
import { Router } from '@angular/router';


@Component({
  selector: 'app-news',
  templateUrl: './news.component.html',
  styleUrls: ['./news.component.css']
})
export class NewsComponent extends PagedListingComponentBase<NewsDto>{
  

  constructor(
    injector: Injector,
    private router: Router,
    private dialog: MatDialog,
    private manageNewsService: ManageNewsService
  ) {
    super(injector);
  }
  news = [] as NewsDto[];
  pageNumber: number;
  request: PagedRequestDto;
  isLoading: boolean;

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this.isLoading = true;
    this.manageNewsService
    .getAllPagging(request)
    .subscribe(res=> {
      this.request = request;
      this.news = res.result.items;
      this.isLoading = false;
    });
  }
  onScroll() {
     this.request['skipCount'] += this.request['maxResultCount'];
     this.manageNewsService.getAllPagging(this.request).subscribe( res => {
       if (res && res.result && res.result.items) {
         res.result.items.forEach(el => {
           this.news.push(el);
           
         });
       }
     })
  }
  detailNews(id) {
    this.router.navigate(['app/main/news/news-detail', id]);
  }
  protected delete(item: NewsDto): void {
  }
}

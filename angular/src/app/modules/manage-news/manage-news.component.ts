import { Component, OnInit, Injector } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material';
import { ManageNewsService } from '@app/service/api/manage-new.service';
import { PagedRequestDto, PagedListingComponentBase } from '@shared/paged-listing-component-base';
import { finalize } from 'rxjs/operators';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { CreateEditNewsComponent } from './create-edit-news/create-edit-news';
import { NewsDto } from '@app/service/api/model/news-dto';


@Component({
  selector: 'app-manage-news',
  templateUrl: './manage-news.component.html',
  styleUrls: ['./manage-news.component.css'],
  animations: [appModuleAnimation()]
})
export class ManageNewsComponent extends PagedListingComponentBase<NewsDto> implements OnInit {

  lists = [] as NewsDto[];
  constructor(injector: Injector,
    private router: Router,
    private dialog: MatDialog,
    private manageNewsService: ManageNewsService) {
    super(injector);
  }


 
  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this.manageNewsService
      .getAllPagging(request)
      .pipe(finalize(() => {
        finishedCallback();

      }))

      .subscribe((result: any) => {

        this.lists = result.result.items;

        // console.log('lists', this.lists);
        this.showPaging(result.result, pageNumber);
      });
  }
  protected delete(item: NewsDto): void {
    abp.message.confirm(
      "Delete  '" + item.title + "'?",
      (result: boolean) => {
        if (result) {
          this.manageNewsService.delete(item.id).subscribe(() => {
            abp.notify.info('Deleted : ' + item.title);
            this.refresh();
          });
        }
      }
    );
  }
  showDialog(list: NewsDto): void {
    let item = { id: list.id, title: list.title, content: list.content, isAllowComment: list.isAllowComment, creationTime: list.creationTime, creatorUserId: list.creatorUserId } as NewsDto;
    const dialogRef = this.dialog.open(CreateEditNewsComponent, {
      data: item
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        if (list.id === undefined) {
          this.manageNewsService.save(result).subscribe(() => {
            this.notify.info(this.l('Create Successfully'));
            this.refresh();
          });
        } else {
          this.manageNewsService.save(result).subscribe(() => {
            this.notify.info(this.l('Saved Successfully'));
            this.refresh();
          });
        }
      }
    });
  }
  createNews(): void {
    let news = {} as NewsDto
    this.showDialog(news)
  }
  editNews(news): void {
    this.showDialog(news);
  }
  detailNews(id) {
    this.router.navigate(['app/main/managenews/manage-detail', id]);
  }  
}



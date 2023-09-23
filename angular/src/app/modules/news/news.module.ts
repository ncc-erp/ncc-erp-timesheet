import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NewsComponent } from './news.component';
import { NewsRoutingModule } from './news-routing.module';
import { FormsModule } from '@angular/forms';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '@shared/shared.module';
import { NewsDetailComponent } from './news-detail/news-detail.component';

// import { ManageNewsDetailComponent } from '@shared/manage-news-detail/manage-news-detail.component';



@NgModule({
  declarations: [
    NewsComponent,
    NewsDetailComponent,
    // ManageNewsDetailComponent
  ],
  imports: [
    CommonModule,
    NewsRoutingModule,
    FormsModule,
    NgxPaginationModule,
    SharedModule
  ]
})
export class NewsModule { }

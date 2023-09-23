import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ManageNewsComponent } from './manage-news.component';
import { ManageNewsRoutingModule } from './manage-news-routing.module';
import { CreateEditNewsComponent } from './create-edit-news/create-edit-news';
import { FormsModule } from '@angular/forms';
import { NgxPaginationModule } from 'ngx-pagination';
import { HttpClientModule} from '@angular/common/http';
import { ManageDetailComponent } from './manage-detail/manage-detail.component';
import { AngularEditorModule } from '@kolkov/angular-editor';

@NgModule({
  declarations: [ManageNewsComponent, CreateEditNewsComponent, ManageDetailComponent ],
  imports: [
    CommonModule,
    ManageNewsRoutingModule,
    SharedModule,
    FormsModule,
    NgxPaginationModule,
    HttpClientModule,
    AngularEditorModule
  ],
  entryComponents: [
    CreateEditNewsComponent
  ],
  exports: [
    CreateEditNewsComponent
  ]
 
})
export class ManageNewsModule { }
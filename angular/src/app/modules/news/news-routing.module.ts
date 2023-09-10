import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { NewsComponent } from './news.component';
import { NewsDetailComponent } from './news-detail/news-detail.component';

const routes: Routes = [
    {
      path: '',
      component: NewsComponent,     
    },
    {
      path: 'news-detail/:id',
      component: NewsDetailComponent,     
    }
]
@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
  })
  export class NewsRoutingModule { }
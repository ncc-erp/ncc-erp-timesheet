import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ManageNewsComponent } from './manage-news.component';
import { ManageDetailComponent } from './manage-detail/manage-detail.component';
const routes: Routes = [
    {
      path: '',
      component: ManageNewsComponent,     
    },
    // {
    //   path: 'manage-detail',
    //   component: ManageDetailComponent,     
    // },
    {
      path: 'manage-detail/:id',
      component: ManageDetailComponent,     
    }
]
@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
  })
  export class ManageNewsRoutingModule { }
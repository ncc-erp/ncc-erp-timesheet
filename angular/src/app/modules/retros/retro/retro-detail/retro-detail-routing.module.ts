import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { RetroDetailComponent } from './retro-detail.component';

const routes: Routes = [{
  path: '',
  component: RetroDetailComponent
}]
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class RetroDetailRoutingModule { }

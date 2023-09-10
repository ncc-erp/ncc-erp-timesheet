import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InternsInfoComponent } from './interns-info.component';

const routes: Routes = [
  { path: '', component: InternsInfoComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class InternsInfoRoutingModule { }


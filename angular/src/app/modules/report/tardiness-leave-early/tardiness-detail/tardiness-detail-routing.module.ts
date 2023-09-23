import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { TardinessDetailComponent } from './tardiness-detail.component';

const routes: Routes = [{
  path: '',
  component: TardinessDetailComponent
}]
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class TardinessDetailRoutingModule { }

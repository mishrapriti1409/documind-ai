import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { CompareComponent } from './compare.component';

const routes: Routes = [
  { path: '', component: CompareComponent }
];

@NgModule({
  declarations: [CompareComponent],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild(routes)
  ]
})
export class CompareModule {}


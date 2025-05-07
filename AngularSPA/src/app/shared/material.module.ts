import { NgModule } from '@angular/core';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTabsModule } from '@angular/material/tabs';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { ClipboardModule } from '@angular/cdk/clipboard';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialogModule } from '@angular/material/dialog';

@NgModule({
  exports: [
    MatExpansionModule,
    DragDropModule,
    MatDialogModule,
    ScrollingModule,
    MatTabsModule,
    ClipboardModule,
    MatCardModule,
    MatProgressBarModule
  ],
  imports: [
    MatExpansionModule,
    DragDropModule,
    MatDialogModule,
    ScrollingModule,
    MatTabsModule,
    ClipboardModule,
    MatCardModule,
    MatProgressBarModule
  ]
})
export class MaterialModule { }

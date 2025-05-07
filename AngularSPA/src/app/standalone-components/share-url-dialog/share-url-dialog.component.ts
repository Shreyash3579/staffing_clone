import {Component, inject} from "@angular/core";
import {MAT_DIALOG_DATA, MatDialogActions, MatDialogContent, MatDialogRef, MatDialogTitle} from "@angular/material/dialog";
import {NgIf} from "@angular/common";
import {MatSnackBar} from "@angular/material/snack-bar";

@Component({
  selector: "app-share-url-dialog",
  standalone: true,
  imports: [MatDialogContent, MatDialogActions, NgIf, MatDialogTitle],
  templateUrl: "./share-url-dialog.component.html",
  styleUrl: "./share-url-dialog.component.scss"
})
export class ShareUrlDialogComponent {
  readonly dialogRef = inject(MatDialogRef<ShareUrlDialogComponent>);
  readonly data = inject<{
    url: string;
  }>(MAT_DIALOG_DATA);
  showUrl = false;
  url = "";
  private _snackBar = inject(MatSnackBar);

  onNoClick(): void {
    this.dialogRef.close();
  }

  copyUrlToClipboard(): void {
    navigator.clipboard.writeText(this.url).then(() => {
      this.openSnackBar("URL copied to clipboard", "");
      console.log("URL copied to clipboard");
    }).catch(err => {
      this.openSnackBar("Failed to copy URL: ", "");
      console.error("Failed to copy URL: ", err);
    });
  }

  openSnackBar(message: string, action: string) {
    this._snackBar.open(message, action, {duration: 1000});
  }

  createUrl() {
    this.showUrl = true;
    this.url = this.data.url; // TODO: Implement URL snapshot endpoint
  }
}

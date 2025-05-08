import { Component,  EventEmitter, Input, Output, OnChanges } from '@angular/core';

@Component({
  selector: 'app-paginator',
  templateUrl: './paginator.component.html',
  styleUrls: ['./paginator.component.scss']
})
export class PaginatorComponent implements OnChanges {
  pageRange: Array<{}> = [];
  lastPageNumber: number;

  /*------------ Input Variables ----------------*/
  @Input() totalCount: number;
  @Input() resultsPerPage: number;
  @Input() selectedPageNumber: number;

  /*------------ Output Variables ----------------*/
  @Output() pageNavigation = new EventEmitter();

  ngOnChanges() {
    this.resetPageNavigation();
  }

  resetPageNavigation() {
    // Determine the last page number
    this.lastPageNumber = Math.ceil(this.totalCount / this.resultsPerPage);

    // If the number of pages is more than 1 then create the page range else return empty array
    if (this.lastPageNumber > 1) {
      this.pageRange = [];
      for (let i: number = 1; i <= this.lastPageNumber; i++) {

        // Set the visibility of the page according to visibility logic (in separate function) and if the page is immediately preceeded and followed by a visible page
        let isVisible: boolean;
        isVisible = this.isPageVisible(i) || (this.isPageVisible(i + 1) && this.isPageVisible(i - 1));

        let page = {
          number: i,
          isVisible: isVisible,
          isEllipsis: false,
          text: i
        };

        // Add the page
        this.pageRange.push(page);
      }

      // Add any ellipsis elements if required
      this.pageRange = this.addEllipsis(this.pageRange);
    }
    else
      this.pageRange = [];
  }

  isPageVisible(pageNumber: number): boolean {
    let isVisible: boolean = false;

    if (pageNumber > 0 && pageNumber <= this.lastPageNumber) {

      // Set the visibility of page to true if
      //  a) It is the first or last page
      //  b) It is a page immediately preceeding or following the selected page
      //  c) If the selected page is 1 or 2 and the current page is among the first 4 pages
      //  d) It the selected page is lastpage or lastpage -1 and the current page is among the last 4 pages
      
      isVisible = pageNumber === 1 || pageNumber === this.lastPageNumber
        || (pageNumber >= this.selectedPageNumber - 1 && pageNumber <= this.selectedPageNumber + 1)
        || ((this.selectedPageNumber === 1 || this.selectedPageNumber === 2) && pageNumber <= 4)
        || ((this.selectedPageNumber === this.lastPageNumber || this.selectedPageNumber === this.lastPageNumber - 1) && pageNumber > this.lastPageNumber - 4);
    }
    return isVisible;
  }

  addEllipsis(pageRange): Array<{}> {
    let firstNonVisiblePage: number;
    let lastNonVisiblePage: number;
    let middleVisiblePage: number;
    let ellipsis = {
      number: 0,
      isVisible: true,
      isEllipsis: true,
      text: "..."
    }

    // Add ellipsis elements to the page range if there are non-visible page numbers
    firstNonVisiblePage = pageRange.map(p => p["isVisible"]).indexOf(false);
    lastNonVisiblePage = pageRange.map(p => p["isVisible"]).lastIndexOf(false);
    middleVisiblePage = firstNonVisiblePage + pageRange.slice(firstNonVisiblePage + 1).map(p => p["isVisible"]).indexOf(true);

    if (firstNonVisiblePage !== -1) {
      // If the non-visible page numbers are separated by visible pages in between them then insert 2 ellipses on each side
      if (firstNonVisiblePage < middleVisiblePage && middleVisiblePage < lastNonVisiblePage) {        
        pageRange.splice(firstNonVisiblePage, 0, ellipsis);
        // To insert the ellipsis after the lastNonVisiblePage add 2 to the index (+1 for the next index and +1 as an ellipsis element was added in the previous step)
        pageRange.splice(lastNonVisiblePage + 2, 0, ellipsis);
      }
      else {
        // Insert a single ellipsis denoting the entire non-visible page range
        pageRange.splice(firstNonVisiblePage, 0, ellipsis);
      }
    }

    return pageRange;
  }

  getPageResults(page: number) {
    if (page > 0 && page <= this.lastPageNumber && page !== this.selectedPageNumber)
      this.pageNavigation.emit(page);
  }

}

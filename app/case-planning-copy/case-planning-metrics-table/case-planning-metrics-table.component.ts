import { Component, OnInit, Input } from "@angular/core";
import { CasePlanningBoardBucket } from "src/app/shared/interfaces/case-planning-board-bucket.interface";

@Component({
  selector: "app-case-planning-metrics-table",
  templateUrl: "./case-planning-metrics-table.component.html",
  styleUrls: ["./case-planning-metrics-table.component.scss"]
})
export class CasePlanningMetricsTableComponent implements OnInit {
  // inputs
  @Input() planningBoardColumnMetrics;
  @Input() planningBoard;

  // local variables
  metricsBodyExpandedRowsIds = [];
  metricsLowerLevelExpandedRowsIdWithHeight = {}; //{"<rowId>": "<rowHeight>"}

  constructor() {}

  ngOnInit(): void {}

  ngOnChanges(): void {
    console.log(this.planningBoardColumnMetrics);
  }


  // Expand All Rows
  toggleAllRows() {
    const allRowElements = document.querySelectorAll<HTMLElement>(".metrics-body-row");

    allRowElements.forEach((row) => {
      if (row.classList.contains("collapsed")) {
        row.classList.remove("collapsed");
      } else {
        row.classList.add("collapsed");
      }
    });
  }

  /* Expand / Collapse Top rows which includes
   ** Metrics
   ** Staff from Supply
   ** Action Needed
   ** Others
   */
  toggleCasePlanningBoardRows(event, rowNumber) {
    const rowElements = document.querySelectorAll<HTMLElement>(".data-row-" + rowNumber);

    rowElements.forEach((item) => {
      item.style.transition = "all 0.2s ease-in-out";
      item.style.overflow = "hidden";
    });

    // Collapse / Expand Row
    if (event.currentTarget.classList.contains("collapsed")) {
      event.currentTarget.classList.remove("collapsed");
      rowElements.forEach((item) => {
        item.classList.remove("collapsed");
        item.style.maxHeight = "999px";
      });
    } else {
      event.currentTarget.classList.add("collapsed");
      rowElements.forEach((item) => {
        item.classList.add("collapsed");
        item.style.maxHeight = "24px";
      });
    }
  }

  /* Toggle Metrics Body Level 1 & Level 2 Rows
   ** Supply, Demand, Balance Rows
   ** Team, SMAP, etc rows within Supply
   */
  toggleMetricsUpperLevelBody(toggleElementName, event, rowIndex) {
    const rowId = `${toggleElementName}-body-row-${rowIndex}`;
    if (this.metricsBodyExpandedRowsIds.includes(rowId)) {
      this.metricsBodyExpandedRowsIds.splice(this.metricsBodyExpandedRowsIds.indexOf(rowId), 1);
    } else {
      this.metricsBodyExpandedRowsIds.push(rowId);
    }

    const rowElements = document.querySelectorAll<HTMLElement>(`#${rowId}`);

    if (event.currentTarget.classList.contains("collapsed")) {
      rowElements.forEach((row) => {
        // row.classList.remove("collapsed");
      });
    } else {
      rowElements.forEach((row) => {
        // row.classList.add("collapsed");
      });
    }
  }

  /* Toggle Metrics Body Level 3 Rows
   ** Individual Levels like A,C, M etc
   */
  toggleMetricsLowerLevelBody(toggleElementName, event, rowIndex) {
    const rowId = `${toggleElementName}-body-row-${rowIndex}`;
    if (this.metricsBodyExpandedRowsIds.includes(rowId)) {
      this.metricsBodyExpandedRowsIds.splice(this.metricsBodyExpandedRowsIds.indexOf(rowId), 1);
    } else {
      this.metricsBodyExpandedRowsIds.push(rowId);
    }

    const rowElements = document.querySelectorAll<HTMLElement>(`#${rowId}`);

    const rowHeights = []; // Used for row column heights

    if (event.currentTarget.classList.contains("collapsed")) {
      rowElements.forEach((row) => {
        row.classList.remove("collapsed");
        rowHeights.push(row.offsetHeight); // Get heights of expanded level columns
      });

      const maxHeight = Math.max(...rowHeights); // Get the max value
      // rowElements[0].style.height = maxHeight + "px"; // Set value of first left side column to max height when expanded
      rowElements.forEach((row) => {
        row.style.height = maxHeight + "px";
      });

      this.metricsLowerLevelExpandedRowsIdWithHeight[rowId] = maxHeight + "px";
    } else {
      rowElements[0].style.height = "24px";
      delete this.metricsLowerLevelExpandedRowsIdWithHeight[rowId]; //delete the property from object whene collapsed

      rowElements.forEach((row) => {
        row.classList.add("collapsed");
      });
    }
  }

  // Toggle Include in Demand switch
  toggleIncludeInDemand(bucket: CasePlanningBoardBucket) {
    // bucket.includeInDemand = !bucket.includeInDemand;
    // bucket.isPartiallyChecked = false;
    // this.updateCasePlanningBoardBucketPreferencesInLocalStorage(bucket);
    // this.upsertCasePlanningBoardBucketPreferences(bucket);
    // this.planningBoard.forEach((planningBoardColumn) => {
    //   let includeInDemandProjects = planningBoardColumn.buckets.find(
    //     (x) => x.bucketId == CasePlanningBoardBucketEnum.ACTION_NEEDED
    //   ).projects;
    //   includeInDemandProjects.forEach((project) => {
    //     project.includeInDemand = bucket.includeInDemand;
    //     const projectInDemandMetricsProjects = this.demandMetricsProjects.filter(
    //       (x) => project.bucketId === CasePlanningBoardBucketEnum.ACTION_NEEDED
    //     );
    //     projectInDemandMetricsProjects.forEach((project) => {
    //       project.includeInDemand = bucket.includeInDemand;
    //     });
    //   });
    // });
    // this.createMetricsForSupplyAndDemand();
  }

  toggleIncludeProjectInDemand(project, bucket) {
    bucket.includeInDemand = true;

    // const projectInDemandMetricsProjects = this.demandMetricsProjects.find(
    //   (x) => project.planningBoardId && x.planningBoardId === project.planningBoardId
    // );
    // projectInDemandMetricsProjects.includeInDemand = project.includeInDemand;

    // this.upsertCasePlanningBoardIncludeInDemandUserPreference(project, bucket);
    // this.createMetricsForSupplyAndDemand();
  }

  toggleIndividualCountForSupplyMetrics() {
    // this.isCountOfIndividualResourcesToggle = !this.isCountOfIndividualResourcesToggle;
    // this.createMetricsForSupplyAndDemand();
  }

  toggleHighlightNewlyAvailable() {
    // this.enableNewlyAvailableHighlighting = !this.enableNewlyAvailableHighlighting;
  }

  toggleGroupByDateAvailable() {
    // this.enableMemberGrouping = !this.enableMemberGrouping;
  }
}
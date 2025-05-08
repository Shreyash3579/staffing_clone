import { Component, EventEmitter, HostListener, Input, OnInit, Output, SimpleChanges } from "@angular/core";

@Component({
  selector: "app-metrics-table",
  templateUrl: "./metrics-table.component.html",
  styleUrls: ["./metrics-table.component.scss"]
})
export class MetricsTableComponent implements OnInit {
  @Input() metrics;
  @Input() weekOf;
  @Input() metricsBodyExpandedRowsIds = [];
  @Input() metricsLowerLevelExpandedRowsIdWithHeight = {};
  @Input() playgroundId: string;
  @Output() openAddCommitment = new EventEmitter();
  @Output() openQuickAddForm = new EventEmitter();
  @Output() openStaffableTeams = new EventEmitter();

  public windowWidth;

  public teamLabel = "Team";
  public smapLabel = "Smap+";
  public partnerLabel = "Partner";
  public addLabel = "Add";

  public teamSum;
  isGcTeamCountVisible: boolean = true;
  isPegTeamCountVisible: boolean = true;
  worldwideCount: number = 0;
  @Input() groupByDatesAvailable: boolean = false;
  @Input() highlightNewlyAvailable: boolean = false;

  constructor() { }

  ngOnInit(): void {
    this.getSupplyDemandBalanceAndWorldWideSum();
  }

  getSupplyDemandBalanceAndWorldWideSum() {
    // Get Sum of Supply, Demand, and Balance
    for (var i in this.metrics) {
      const initialValue = 0;
      const dataLevelOne = this.metrics[i].data;
      const metricsId = this.metrics[i].id;

      if (metricsId === 'supply') {
        for (var j in dataLevelOne) {
          const dataAvailable = dataLevelOne[j].available;
          const dataProspective = dataLevelOne[j].prospective;

          for (var k in dataAvailable) {
            this.getSumForSupplyLevels(dataAvailable[k], initialValue);
          }

          for (var k in dataProspective) {
            this.getSumForSupplyLevels(dataProspective[k], initialValue);
          }

          dataLevelOne[j]["sum"] = (dataAvailable.reduce(
            (previousValue, currentValue) => Number(parseFloat(previousValue)) + Number(parseFloat(currentValue.sum)),
            initialValue
          ) +
            dataProspective.reduce(
              (previousValue, currentValue) => Number(parseFloat(previousValue)) + Number(parseFloat(currentValue.sum)),
              initialValue
            )).toFixed(2);
        }
      }
      else if(metricsId === 'demand') {
        for (var j in dataLevelOne) {
          let levelSum;
          levelSum = dataLevelOne[j].levels.reduce(
            (previousValue, currentValue) => Number(parseFloat(previousValue)) + Number(parseFloat(currentValue.sum)),
            initialValue
          ) 
          dataLevelOne[j]["sum"] = parseFloat(levelSum).toFixed(2);
        }
      }
      else if (metricsId === 'balance') {
        for (var j in dataLevelOne) {

          dataLevelOne[j]["sum"] = (this.metrics[0].data[j]["sum"] - this.metrics[1].data[j]["sum"]).toFixed(2);

          dataLevelOne[j]["levels"]?.forEach((level) => {
              const supplyAvailableCountForLevel = this.metrics[0].data[j].available[0].levels.find(x => x.name === level.name).sum;
              const supplyProspectiveCountForLevel = this.metrics[0].data[j].prospective[0].levels.find(x => x.name === level.name).sum;
              const supplyTotalCountForLevel = Number(parseFloat(supplyAvailableCountForLevel)) + Number(parseFloat(supplyProspectiveCountForLevel));
              const demandTotalCountForLevel = Number(parseFloat(this.metrics[1].data[j].levels.find(x => x.name === level.name)? 
              this.metrics[1].data[j].levels.find(x => x.name === level.name)["sum"] : 0));

              const levelSum = supplyTotalCountForLevel - demandTotalCountForLevel;
              level["sum"] = parseFloat(levelSum.toString()).toFixed(2);
          });
        }
      }

      if (metricsId === 'staffableTeams') {
        this.metrics[i]["sum"] = dataLevelOne[0].sum;
        // this.worldwideCount += this.isGcTeamCountVisible ? dataLevelOne[0].staffableTeams.gcTeamCount : 0;
        // this.worldwideCount += this.isPegTeamCountVisible ? dataLevelOne[0].staffableTeams.pegTeamCount : 0;
      }
      else {
        this.metrics[i]["sum"] = dataLevelOne?.reduce(
          (previousValue, currentValue) => (Number(parseFloat(previousValue)) + Number(parseFloat(currentValue.sum))).toFixed(2),
          initialValue
        );
      }
    }
  }
 
  getSumForSupplyLevels(data, initialValue){
    let levelSum;
    const levels = data.levels;
    levels?.forEach((item) => {
      levelSum = item.levelGrades.reduce(
        (previousValue, currentValue) => (Number(parseFloat(previousValue)) + Number(parseFloat(currentValue.supply))).toFixed(2),
        initialValue
      );
      item["sum"] = parseFloat(levelSum).toFixed(2);
      this.groupDataByDatesAvailable(item);
    });
    data["sum"] = levels.reduce(
      (previousValue, currentValue) => (Number(parseFloat(previousValue)) + Number(parseFloat(currentValue.sum))).toFixed(2),
      initialValue
    );
  }

  highlightNewlyAvailableData(member) {
    if(this.highlightNewlyAvailable && member.highlight) {
      return true;
    }
    else 
    return false;
  }

  groupDataByDatesAvailable(item) {
    let groups = {};
    item.levelGrades.forEach((level) => {
      level.members?.forEach(member => {
        this.createGroupedMembers(member, groups);
      });
    });

    item["groupedMembers"] = Object.keys(groups).map((date) => {
      return {
        date,
        members: groups[date]
      };
    });

    item["groupedMembers"].sort((a,b) => a.date.localeCompare(b.date));
  }

  createGroupedMembers(member, groups) {
    let date = member.availabilityDate.split("T")[0];
    if (date in groups) {
      groups[date].push(member);
    } else {
      groups[date] = [member];
    }
  }

  openAddCommitmentHandler() {
    this.openAddCommitment.emit();
  }

  openStaffableTeamsHandlerForWeek() {
    this.openStaffableTeams.emit(this.weekOf);
  }

  @HostListener("window:resize", ["$event"])
  onResize(event) {
    this.windowWidth = event.target.innerWidth;
    this.labelsToDisplay(this.windowWidth);
  }

  labelsToDisplay(width) {
    if (width < 1650) {
      this.teamLabel = "T.";
      this.smapLabel = "S.+";
      this.partnerLabel = "P.";
      this.addLabel = "Add.";
    } else {
      this.teamLabel = "Team";
      this.smapLabel = "Smap+";
      this.partnerLabel = "Partner";
      this.addLabel = "Add.";
    }
  }

  openQuickAddFormHandler(event, employee) {
    if (employee.employeeCode) {
      this.openQuickAddForm.emit({
        employeeCode: employee.employeeCode
      });
      this.openQuickAddForm.emit(event);
    }
  }
}

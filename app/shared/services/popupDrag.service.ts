import { Injectable } from "@angular/core";
@Injectable()
export class PopupDragService
{
  constructor() {
  }

  // -----------------------Event Handlers--------------------------------------------//
  public  dragEvents() {
    var modalContainerElement: HTMLElement;
    var modalBodyFormElement: HTMLElement;
    var popupDragNotEffectedElements;
    var isDown = false;
    var initX, initY, mousePressX, mousePressY;
    modalBodyFormElement = document.getElementsByClassName('modal-body')[0] as HTMLElement;
    modalBodyFormElement.classList.add("popup-drag-not-effected");
    modalContainerElement = document.getElementsByClassName('modal-content')[0] as HTMLElement;
    popupDragNotEffectedElements = document.getElementsByClassName('popup-drag-not-effected');
    modalContainerElement.addEventListener("mousedown", ($event) => {
      initX = modalContainerElement.offsetLeft;
      initY = modalContainerElement.offsetTop;
      mousePressX = $event.clientX;
      mousePressY = $event.clientY;
      isDown = true;
    })
    window.document.addEventListener("mousemove", ($event) => {
      if (isDown) {
        if ($event.screenY < window.screen.height - 100) {
          if ($event.screenY > 100) {
            modalContainerElement.style.top = initY + $event.clientY - mousePressY + 'px';
          }
        }
        if ($event.screenX < window.screen.width - 100) {
          if ($event.screenX > 100) {
            modalContainerElement.style.left = initX + $event.clientX - mousePressX + 'px';
          }
        }
      }
    })
    window.document.addEventListener("mouseup", () => {
      isDown = false;
    })
    for (let element = 0; element < popupDragNotEffectedElements.length; element++) {
      const elementHtml = popupDragNotEffectedElements[element] as HTMLElement;
      elementHtml.addEventListener("mousedown", (e) => {
        isDown = false;
        event.stopPropagation();
      });
      elementHtml.addEventListener("mousemove", (e) => {
        isDown = false;
        event.stopPropagation();
      });
    }
  }
}

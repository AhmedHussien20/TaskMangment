import { TaskExtensionRequestGet } from "./task-extension-request";
import { TaskCloseRequestGet } from "./task-close-request";
import { TaskCommentGetDto } from "./task-comment";
import { WarningGetDto } from "./task-warning";
import { DiscountGetDto } from "./task-penalty";
import { TaskPercentageGetDto } from "./task-percentage";

export interface TaskActivitySummary {
  taskId: number;

  commentsCount: number;
  lastComment?: TaskCommentGetDto;

  warningsCount: number;
  lastWarning?: WarningGetDto;

  penaltysCount: number;
  lastPenalty?: DiscountGetDto

  extensionRequestsCount: number;
  lastExtensionRequest?: TaskExtensionRequestGet;

  closeRequestsCount: number;
  lastCloseRequest?: TaskCloseRequestGet;

  percentageCount: number;
  lastPercentage?: TaskPercentageGetDto
}

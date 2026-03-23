import { NextRequest, NextResponse } from "next/server";
import { getIdems, getTotalCount, initializeDatabase } from "@/lib/db";
import { demoIdems } from "@/lib/demo-data";
import type { PaginatedIdems } from "@/types/idem";

const DEFAULT_PAGE_SIZE = 20;

// Initialize database on first request
let initialized = false;

function getDemoPage(page: number, pageSize: number): PaginatedIdems {
  const start = (page - 1) * pageSize;
  const items = demoIdems.slice(start, start + pageSize);
  const totalCount = demoIdems.length;
  const hasMore = start + pageSize < totalCount;

  return {
    items,
    page,
    pageSize,
    totalCount,
    hasMore,
    nextPage: hasMore ? page + 1 : null,
    demoMode: true,
  };
}

export async function GET(request: NextRequest) {
  const searchParams = request.nextUrl.searchParams;
  const page = parseInt(searchParams.get("page") || "1", 10);
  const pageSize = parseInt(
    searchParams.get("pageSize") || String(DEFAULT_PAGE_SIZE),
    10
  );
  const includeSeeded = searchParams.get("include_seeded") !== "false";

  if (page < 1 || isNaN(page)) {
    return NextResponse.json(
      { error: "Invalid page number", message: "Page must be a positive integer" },
      { status: 400 }
    );
  }

  if (pageSize < 1 || pageSize > 100 || isNaN(pageSize)) {
    return NextResponse.json(
      { error: "Invalid page size", message: "Page size must be between 1 and 100" },
      { status: 400 }
    );
  }

  try {
    // Initialize database schema on first request
    if (!initialized) {
      await initializeDatabase();
      initialized = true;
    }

    const [items, totalCount] = await Promise.all([
      getIdems(page, pageSize, includeSeeded),
      getTotalCount(includeSeeded),
    ]);

    const hasMore = page * pageSize < totalCount;

    const response: PaginatedIdems = {
      items,
      page,
      pageSize,
      totalCount,
      hasMore,
      nextPage: hasMore ? page + 1 : null,
    };

    return NextResponse.json(response);
  } catch (error) {
    console.error("Database unavailable, falling back to demo data:", error);
    return NextResponse.json(getDemoPage(page, pageSize));
  }
}

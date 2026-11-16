"""RAG evaluation runner script producing structured accuracy & latency reports."""
from __future__ import annotations
import asyncio
import json
from pathlib import Path
from evaluation.dataset import EVAL_DATASET
from evaluation.metrics import evaluate_single


async def run_evaluation(organization_id, document_ids=None):
    from app.services.rag_engine import answer_question

    results = []
    print(f"Starting RAG evaluation across {len(EVAL_DATASET)} benchmark test cases...")

    for i, item in enumerate(EVAL_DATASET, start=1):
        print(f"[{i}/{len(EVAL_DATASET)}] Evaluating ({item['category']}): {item['question'][:60]}...")
        try:
            metrics = await evaluate_single(
                question=item["question"],
                expected_topics=item["expected_topics"],
                category=item["category"],
                rag_fn=answer_question,
                organization_id=organization_id,
                document_ids=document_ids,
                conversation_history=None,
            )
            results.append({
                "index": i,
                "question": item["question"],
                "category": item["category"],
                **vars(metrics)
            })
        except Exception as e:
            results.append({
                "index": i,
                "question": item["question"],
                "category": item["category"],
                "error": str(e)
            })

    valid = [r for r in results if "error" not in r]
    count = len(valid) or 1

    report = {
        "total_test_cases": len(results),
        "successful_evaluations": len(valid),
        "failed_evaluations": len(results) - len(valid),
        "aggregate_metrics": {
            "avg_retrieval_precision": round(sum(r["retrieval_precision"] for r in valid) / count, 3),
            "avg_retrieval_recall": round(sum(r["retrieval_recall"] for r in valid) / count, 3),
            "avg_answer_faithfulness": round(sum(r["answer_faithfulness"] for r in valid) / count, 3),
            "avg_answer_relevance": round(sum(r["answer_relevance"] for r in valid) / count, 3),
            "avg_citation_accuracy": round(sum(r["citation_accuracy"] for r in valid) / count, 3),
            "avg_response_latency_ms": round(sum(r["response_latency_ms"] for r in valid) / count, 2),
            "total_estimated_cost_usd": round(sum(r["estimated_cost_usd"] for r in valid), 6),
        },
        "per_category_metrics": {},
        "test_results": results
    }

    # Group metrics by category
    categories = set(r["category"] for r in valid)
    for cat in categories:
        cat_items = [r for r in valid if r["category"] == cat]
        cat_count = len(cat_items) or 1
        report["per_category_metrics"][cat] = {
            "count": len(cat_items),
            "avg_precision": round(sum(r["retrieval_precision"] for r in cat_items) / cat_count, 3),
            "avg_faithfulness": round(sum(r["answer_faithfulness"] for r in cat_items) / cat_count, 3),
            "avg_latency_ms": round(sum(r["response_latency_ms"] for r in cat_items) / cat_count, 2),
        }

    out_file = Path("evaluation_report.json")
    out_file.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(f"\nEvaluation Complete! Report written to {out_file.resolve()}")
    print(f"Summary: Precision={report['aggregate_metrics']['avg_retrieval_precision']}, Faithfulness={report['aggregate_metrics']['avg_answer_faithfulness']}")
    return report


if __name__ == "__main__":
    import sys
    org_id = sys.argv[1] if len(sys.argv) > 1 else "11111111-2222-3333-4444-555555555555"
    asyncio.run(run_evaluation(org_id))

